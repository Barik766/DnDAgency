using DnDAgency.Application.DTOs.BookingsDTO;
using DnDAgency.Application.DTOs.UsersDTO;
using DnDAgency.Application.Interfaces;
using DnDAgency.Domain.Entities;
using DnDAgency.Domain.Exceptions;
using DnDAgency.Domain.Interfaces;
using DnDAgency.Infrastructure.Interfaces;

namespace DnDAgency.Application.Services
{
    public class BookingService : IBookingService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConflictCheckService _conflictCheckService;

        public BookingService(
            IUnitOfWork unitOfWork,
            IConflictCheckService conflictCheckService)
        {
            _unitOfWork = unitOfWork;
            _conflictCheckService = conflictCheckService;
        }

        public async Task<BookingDto> CreateBookingAsync(Guid userId, Guid campaignId, DateTime startTime, int playersCount = 1)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                // Преобразуем в UTC если Kind не указан
                if (startTime.Kind == DateTimeKind.Unspecified)
                {
                    startTime = DateTime.SpecifyKind(startTime, DateTimeKind.Utc);
                }

                // Валидация playersCount
                if (playersCount < 1 || playersCount > 8)
                    throw new ArgumentException("Players count must be between 1 and 8");

                // Проверяем существование пользователя
                var user = await _unitOfWork.Users.GetByIdAsync(userId);
                if (user == null)
                    throw new KeyNotFoundException("User not found");

                // Проверяем существование кампании с полной загрузкой
                var campaign = await _unitOfWork.Campaigns.GetByIdAsync(campaignId);
                if (campaign == null)
                    throw new KeyNotFoundException("Campaign not found");

                if (!campaign.DurationHours.HasValue)
                    throw new InvalidOperationException("Campaign duration is not set");

                var duration = TimeSpan.FromHours(campaign.DurationHours.Value);

                // Проверяем конфликты времени
                var hasConflict = await _conflictCheckService.HasConflictAsync(campaignId, startTime, duration);
                if (hasConflict)
                    throw new InvalidOperationException("Time slot conflicts with existing bookings");

                // Проверяем, есть ли уже слот на это время
                var existingSlot = await _unitOfWork.Slots.GetByCampaignAndTimeAsync(campaignId, startTime);

                Slot slot;

                if (existingSlot != null)
                {
                    // Проверяем, может ли пользователь забронировать этот слот
                    if (existingSlot.IsInPast)
                        throw new PastSlotBookingException();

                    // Проверяем, нет ли уже брони у этого пользователя на этот слот
                    if (existingSlot.Bookings.Any(b => b.UserId == userId))
                        throw new ArgumentException("User already has booking for this time slot");

                    // Проверяем, хватает ли места для указанного количества игроков
                    if (existingSlot.AvailableSlots < playersCount)
                        throw new ArgumentException($"Not enough available slots. Available: {existingSlot.AvailableSlots}, Requested: {playersCount}");

                    slot = existingSlot;
                }
                else
                {
                    // Валидация рабочих часов перед созданием слота
                    var timeOfDay = startTime.TimeOfDay;
                    if (timeOfDay < campaign.WorkingHoursStart)
                        throw new ArgumentException($"Start time cannot be earlier than {campaign.WorkingHoursStart}");
                    if (timeOfDay > campaign.GetMaxStartTime())
                        throw new ArgumentException($"Start time cannot be later than {campaign.GetMaxStartTime()}");

                    // Проверяем, хватает ли места в кампании для указанного количества игроков
                    if (campaign.MaxPlayers < playersCount)
                        throw new ArgumentException($"Not enough slots in campaign. Max players: {campaign.MaxPlayers}, Requested: {playersCount}");

                    // Создаем новый слот
                    slot = new Slot(campaignId, startTime);

                    // Проверяем, нет ли у пользователя уже брони на эту дату
                    var userBookingOnDate = campaign.Slots
                        .Where(s => s.StartTime.Date == startTime.Date)
                        .SelectMany(s => s.Bookings)
                        .Any(b => b.UserId == userId);

                    if (userBookingOnDate)
                        throw new ArgumentException("User already has booking on this date");

                    await _unitOfWork.Slots.AddAsync(slot);
                    await _unitOfWork.SaveChangesAsync(); // Сохраняем слот, чтобы получить ID
                }

                // Создаем бронирование с указанием количества игроков
                var booking = new Booking(userId, slot.Id, playersCount);
                await _unitOfWork.Bookings.AddAsync(booking);
                await _unitOfWork.SaveChangesAsync();

                await _unitOfWork.CommitTransactionAsync();

                // Перезагружаем бронирование с полными данными
                booking = await _unitOfWork.Bookings.GetByIdAsync(booking.Id);
                return MapToDto(booking!);
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<List<BookingDto>> GetUserBookingsAsync(Guid userId)
        {
            var bookings = await _unitOfWork.Bookings.GetByUserIdAsync(userId);
            return bookings.Select(MapToDto).ToList();
        }

        public async Task CancelBookingAsync(Guid bookingId, Guid userId)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var booking = await _unitOfWork.Bookings.GetByIdAsync(bookingId);
                if (booking == null)
                    throw new KeyNotFoundException("Booking not found");

                if (booking.UserId != userId)
                    throw new UnauthorizedAccessException("Cannot cancel booking of another user");

                if (booking.Slot.StartTime < DateTime.UtcNow)
                    throw new ArgumentException("Cannot cancel booking for past slot");

                // Проверяем, можно ли отменить бронь (например, за 2 часа до начала)
                var hoursBeforeStart = (booking.Slot.StartTime - DateTime.UtcNow).TotalHours;
                if (hoursBeforeStart < 2)
                    throw new ArgumentException("Cannot cancel booking less than 2 hours before start");

                _unitOfWork.Bookings.Delete(booking);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<List<SlotDto>> GetAvailableSlotsAsync(Guid campaignId)
        {
            var slots = await _unitOfWork.Slots.GetAvailableSlotsByCampaignIdAsync(campaignId);
            var availableSlots = new List<SlotDto>();

            foreach (var slot in slots)
            {
                var campaign = slot.Campaign;
                if (!campaign.DurationHours.HasValue)
                    continue;

                var slotDuration = TimeSpan.FromHours(campaign.DurationHours.Value);

                // Проверяем, не в прошлом ли слот и есть ли свободные места
                if (!CanBeBooked(slot))
                    continue;

                // Проверяем пересечение со всеми кампаниями в той же комнате / с мастерами
                var hasConflict = await _conflictCheckService.HasConflictAsync(campaignId, slot.StartTime, slotDuration);

                // Если конфликта нет, добавляем в доступные
                if (!hasConflict)
                    availableSlots.Add(MapSlotToDto(slot));
            }

            return availableSlots;
        }

        // --- Helpers ---

        private static bool CanBeBooked(Slot slot)
        {
            var maxPlayers = slot.Campaign.MaxPlayers;
            var bookedPlayers = slot.Bookings.Count;
            return slot.StartTime > DateTime.UtcNow && bookedPlayers < maxPlayers;
        }

        private static BookingDto MapToDto(Booking booking)
        {
            return new BookingDto
            {
                Id = booking.Id,
                UserId = booking.UserId,
                User = new UserDto
                {
                    Id = booking.User.Id,
                    Username = booking.User.Username,
                    Email = booking.User.Email,
                    Role = booking.User.Role.ToString(),
                    IsMaster = booking.User.IsMaster,
                    IsAdmin = booking.User.IsAdmin
                },
                SlotId = booking.SlotId,
                Slot = MapSlotToDto(booking.Slot),
                CreatedAt = booking.CreatedAt
            };
        }

        private static SlotDto MapSlotToDto(Slot slot)
        {
            var availableSlots = slot.Campaign.MaxPlayers - slot.Bookings.Count;

            return new SlotDto
            {
                Id = slot.Id,
                CampaignId = slot.CampaignId,
                StartTime = slot.StartTime,
                AvailableSlots = availableSlots,
                IsFull = availableSlots <= 0,
                IsInPast = slot.StartTime < DateTime.UtcNow
            };
        }
    }
}