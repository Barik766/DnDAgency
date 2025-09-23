using DnDAgency.Application.DTOs.BookingsDTO;
using DnDAgency.Application.DTOs.UsersDTO;
using DnDAgency.Application.Interfaces;
using DnDAgency.Domain.Entities;
using DnDAgency.Domain.Exceptions;
using DnDAgency.Domain.Interfaces;
using DnDAgency.Infrastructure.Repositories;

namespace DnDAgency.Application.Services
{
    public class BookingService : IBookingService
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly ISlotRepository _slotRepository;
        private readonly IUserRepository _userRepository;
        private readonly ICampaignRepository _campaignRepository; 

        public BookingService(
            IBookingRepository bookingRepository,
            ISlotRepository slotRepository,
            IUserRepository userRepository,
            ICampaignRepository campaignRepository)
        {
            _bookingRepository = bookingRepository;
            _slotRepository = slotRepository;
            _userRepository = userRepository;
            _campaignRepository = campaignRepository;
        }

        public async Task<BookingDto> CreateBookingAsync(Guid userId, Guid campaignId, DateTime startTime)
        {
            // Преобразуем в UTC если Kind не указан
            if (startTime.Kind == DateTimeKind.Unspecified)
            {
                startTime = DateTime.SpecifyKind(startTime, DateTimeKind.Utc);
            }

            // Проверяем существование пользователя
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                throw new KeyNotFoundException("User not found");

            // Проверяем существование кампании с полной загрузкой
            var campaign = await _campaignRepository.GetByIdAsync(campaignId);
            if (campaign == null)
                throw new KeyNotFoundException("Campaign not found");

            // Проверяем, есть ли уже слот на это время
            var existingSlot = await _slotRepository.GetByCampaignAndTimeAsync(campaignId, startTime);

            Slot slot;

            if (existingSlot != null)
            {
                // Проверяем, может ли пользователь забронировать этот слот
                if (existingSlot.IsFull)
                    throw new SlotFullException();

                if (existingSlot.IsInPast)
                    throw new PastSlotBookingException();

                // Проверяем, нет ли уже брони у этого пользователя на этот слот
                if (existingSlot.Bookings.Any(b => b.UserId == userId))
                    throw new ArgumentException("User already has booking for this time slot");

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

                // Создаем новый слот БЕЗ передачи объекта campaign
                slot = new Slot(campaignId, startTime);

                // Остальная проверка...
                var userBookingOnDate = campaign.Slots
                    .Where(s => s.StartTime.Date == startTime.Date)
                    .SelectMany(s => s.Bookings)
                    .Any(b => b.UserId == userId);

                if (userBookingOnDate)
                    throw new ArgumentException("User already has booking on this date");

                await _slotRepository.AddAsync(slot);
            }

            // Создаем бронирование
            var booking = new Booking(userId, slot.Id);
            await _bookingRepository.AddAsync(booking);

            // Перезагружаем бронирование с полными данными
            booking = await _bookingRepository.GetByIdAsync(booking.Id);
            return MapToDto(booking!);
        }

        public async Task<List<BookingDto>> GetUserBookingsAsync(Guid userId)
        {
            var bookings = await _bookingRepository.GetByUserIdAsync(userId);
            return bookings.Select(MapToDto).ToList();
        }

        public async Task CancelBookingAsync(Guid bookingId, Guid userId)
        {
            var booking = await _bookingRepository.GetByIdAsync(bookingId);
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

            await _bookingRepository.DeleteAsync(booking);
        }

        public async Task<List<SlotDto>> GetAvailableSlotsAsync(Guid campaignId)
        {
            var slots = await _slotRepository.GetAvailableSlotsByCampaignIdAsync(campaignId);
            return slots.Select(MapSlotToDto).ToList();
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
