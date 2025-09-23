using DnDAgency.Domain.Entities;
using DnDAgency.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using ApplicationDbContext = DnDAgency.Infrastructure.Data.ApplicationDbContext;

namespace DnDAgency.Infrastructure.Repositories;

public class SlotRepository : GenericRepository<Slot>, ISlotRepository
{
    public SlotRepository(ApplicationDbContext context) : base(context) { }

    public override async Task<Slot?> GetByIdAsync(Guid id)
    {
        return await _dbSet
            .Include(s => s.Campaign)
                .ThenInclude(c => c.Masters) 
            .Include(s => s.Bookings)
                .ThenInclude(b => b.User)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<List<Slot>> GetByCampaignIdAsync(Guid campaignId)
    {
        return await _dbSet
            .Include(s => s.Campaign)
                .ThenInclude(c => c.Masters)
            .Include(s => s.Bookings)
                .ThenInclude(b => b.User)
            .Where(s => s.CampaignId == campaignId)
            .OrderBy(s => s.StartTime)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<List<Slot>> GetAvailableSlotsByCampaignIdAsync(Guid campaignId)
    {
        var now = DateTime.UtcNow;
        return await _dbSet
            .Include(s => s.Bookings)
            .Include(s => s.Campaign)
            .Where(s => s.CampaignId == campaignId &&
                        s.StartTime > now &&
                        s.Bookings.Count < s.Campaign.MaxPlayers)
            .OrderBy(s => s.StartTime)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<List<Slot>> GetUserSlotsAsync(Guid userId)
    {
        return await _dbSet
            .Include(s => s.Campaign)
                .ThenInclude(c => c.Masters)
            .Include(s => s.Bookings.Where(b => b.UserId == userId))
            .Where(s => s.Bookings.Any(b => b.UserId == userId))
            .OrderBy(s => s.StartTime)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<List<Slot>> GetUpcomingSlotsAsync()
    {
        var now = DateTime.UtcNow;
        return await _dbSet
            .Include(s => s.Campaign)
                .ThenInclude(c => c.Masters)
            .Include(s => s.Bookings)
            .Where(s => s.StartTime > now)
            .OrderBy(s => s.StartTime)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<List<Guid>> GetCampaignIdsWithAvailableSlotsAsync(List<Guid> campaignIds)
    {
        var now = DateTime.UtcNow;
        return await _dbSet
            .Include(s => s.Campaign)
            .Where(s => campaignIds.Contains(s.CampaignId) &&
                       s.StartTime > now &&
                       s.Bookings.Count < s.Campaign.MaxPlayers)
            .Select(s => s.CampaignId)
            .Distinct()
            .ToListAsync();
    }

    public async Task<Slot?> GetByCampaignAndTimeAsync(Guid campaignId, DateTime startTime)
    {
        return await _dbSet
            .Include(s => s.Campaign)
                .ThenInclude(c => c.Masters)
            .Include(s => s.Bookings)
                .ThenInclude(b => b.User)
            .FirstOrDefaultAsync(s => s.CampaignId == campaignId &&
                                s.StartTime.Date == startTime.Date &&
                                s.StartTime.TimeOfDay == startTime.TimeOfDay);
    }
}
