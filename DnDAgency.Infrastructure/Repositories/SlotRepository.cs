using DnDAgency.Domain.Entities;
using DnDAgency.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using ApplicationDbContext = DnDAgency.Infrastructure.Data.ApplicationDbContext;

namespace DnDAgency.Infrastructure.Repositories;

public partial class SlotRepository : GenericRepository<Slot>, ISlotRepository
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
            .AsSplitQuery()
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

    public async Task<List<ConflictSlot>> GetBookedSlotsForRoomAndDateAsync(Guid roomId, DateTime date)
    {
        var utcDate = DateTime.SpecifyKind(date.Date, DateTimeKind.Utc);
        return await _dbSet
            .Include(s => s.Campaign)
                .ThenInclude(c => c.Rooms)
            .Include(s => s.Bookings)
            .Where(s => s.Campaign.Rooms.Any(r => r.Id == roomId) &&
                       s.Campaign.IsActive &&
                       s.Campaign.DurationHours.HasValue &&
                       s.StartTime.Date == utcDate.Date &&
                       s.Bookings.Any())
            .Select(s => new ConflictSlot
            {
                Id = s.Id,
                StartTime = s.StartTime,
                EndTime = s.StartTime.AddHours(s.Campaign.DurationHours.Value),
                CampaignId = s.Campaign.Id
            })
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Dictionary<Guid, int>> GetPlayersCountForSlotsAsync(List<Guid> slotIds)
    {
        return await _context.Set<Booking>()
            .Where(b => slotIds.Contains(b.SlotId))
            .GroupBy(b => b.SlotId)
            .Select(g => new { SlotId = g.Key, Count = g.Sum(b => b.PlayersCount) })
            .ToDictionaryAsync(x => x.SlotId, x => x.Count);
    }
}
