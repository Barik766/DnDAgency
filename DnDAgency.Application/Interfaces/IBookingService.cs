using DnDAgency.Application.DTOs.BookingsDTO;

namespace DnDAgency.Application.Interfaces
{
    public interface IBookingService
    {
        Task<List<BookingDto>> GetUserBookingsAsync(Guid userId);
        Task CancelBookingAsync(Guid bookingId, Guid userId);
        Task<List<SlotDto>> GetAvailableSlotsAsync(Guid campaignId);
        Task<BookingDto> CreateBookingAsync(Guid userId, Guid campaignId, DateTime startTime);
    }
}