using DnDAgency.Domain.Entities;

namespace DnDAgency.Domain.Interfaces;

public interface IBookingRepository : IRepository<Booking>
{
    Task<List<Booking>> GetByUserIdAsync(Guid userId);
    Task<Booking?> GetByUserAndSlotAsync(Guid userId, Guid slotId);
    Task<List<Booking>> GetBySlotIdAsync(Guid slotId);
}