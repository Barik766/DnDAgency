using DnDAgency.Application.DTOs.BookingsDTO;
using DnDAgency.Application.DTOs.UsersDTO;
using DnDAgency.Application.Interfaces;
using DnDAgency.Domain.Entities;
using DnDAgency.Domain.Exceptions;
using DnDAgency.Domain.Interfaces;

namespace DnDAgency.Application.Services
{
    public class BookingService : IBookingService
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly ISlotRepository _slotRepository;
        private readonly IUserRepository _userRepository;

        public BookingService(
            IBookingRepository bookingRepository,
            ISlotRepository slotRepository,
            IUserRepository userRepository)
        {
            _bookingRepository = bookingRepository;
            _slotRepository = slotRepository;
            _userRepository = userRepository;
        }

        public async Task<BookingDto> CreateBookingAsync(Guid userId, Guid slotId)
        {
            // Проверяем существование пользователя
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                throw new KeyNotFoundException("User not found");

            // Проверяем существование слота с полной загрузкой
            var slot = await _slotRepository.GetByIdAsync(slotId);
            if (slot == null)
                throw new KeyNotFoundException("Slot not found");

            // Проверяем возможность бронирования
            if (!CanBeBooked(slot))
            {
                if (slot.StartTime < DateTime.UtcNow)
                    throw new PastSlotBookingException();
                if ((slot.Campaign.MaxPlayers - slot.Bookings.Count) <= 0)
                    throw new SlotFullException();
            }

            // Проверяем, нет ли уже брони у этого пользователя на этот слот
            var existingBooking = await _bookingRepository.GetByUserAndSlotAsync(userId, slotId);
            if (existingBooking != null)
                throw new ArgumentException("User already has booking for this slot");

            // Создаем бронирование
            var booking = new Booking(userId, slotId);
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
