using DnDAgency.Domain.Entities;
using DnDAgency.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using ApplicationDbContext = DnDAgency.Infrastructure.Data.ApplicationDbContext;

namespace DnDAgency.Infrastructure.Repositories;

public class BookingRepository : GenericRepository<Booking>, IBookingRepository
{
    public BookingRepository(ApplicationDbContext context) : base(context) { }

    public override async Task<Booking?> GetByIdAsync(Guid id)
    {
        return await _dbSet
            .Include(b => b.User)
            .Include(b => b.Slot)
                .ThenInclude(s => s.Campaign)
                    .ThenInclude(c => c.Master)
            .FirstOrDefaultAsync(b => b.Id == id);
    }

    public async Task<List<Booking>> GetByUserIdAsync(Guid userId)
    {
        return await _dbSet
            .Include(b => b.User)
            .Include(b => b.Slot)
                .ThenInclude(s => s.Campaign)
                    .ThenInclude(c => c.Master)
            .Where(b => b.UserId == userId)
            .OrderByDescending(b => b.CreatedAt)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Booking?> GetByUserAndSlotAsync(Guid userId, Guid slotId)
    {
        return await _dbSet
            .Include(b => b.User)
            .Include(b => b.Slot)
            .FirstOrDefaultAsync(b => b.UserId == userId && b.SlotId == slotId);
    }

    public async Task<List<Booking>> GetBySlotIdAsync(Guid slotId)
    {
        return await _dbSet
            .Include(b => b.User)
            .Include(b => b.Slot)
            .Where(b => b.SlotId == slotId)
            .AsNoTracking()
            .ToListAsync();
    }
}