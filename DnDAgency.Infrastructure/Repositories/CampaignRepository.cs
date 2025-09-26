using DnDAgency.Domain.Entities;
using DnDAgency.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using ApplicationDbContext = DnDAgency.Infrastructure.Data.ApplicationDbContext;

namespace DnDAgency.Infrastructure.Repositories;

public partial class CampaignRepository : GenericRepository<Campaign>, ICampaignRepository
{
    public CampaignRepository(ApplicationDbContext context) : base(context) { }

    // Compiled queries для критических запросов
    private static readonly Func<ApplicationDbContext, Guid, Campaign?> GetByIdWithSlotsCompiledQuery =
        EF.CompileQuery((ApplicationDbContext context, Guid id) =>
            context.Campaigns
                .Include(c => c.Rooms)
                .Include(c => c.Slots)
                .AsNoTracking()
                .FirstOrDefault(c => c.Id == id));

    private static readonly Func<ApplicationDbContext, List<Guid>, IEnumerable<Master>> GetMastersByIdsCompiledQuery =
        EF.CompileQuery((ApplicationDbContext context, List<Guid> masterIds) =>
            context.Masters.Where(m => masterIds.Contains(m.Id)));

    public async Task<Campaign?> GetByIdForUpdateAsync(Guid id)
    {
        return await _dbSet
            .Include(c => c.Masters)
            .Include(c => c.Tags)
            .Include(c => c.Rooms)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public override async Task<Campaign?> GetByIdAsync(Guid id)
    {
        return await _dbSet
            .Include(c => c.Masters)
            .Include(c => c.Tags)
            .Include(c => c.Slots)
                .ThenInclude(s => s.Bookings)
                    .ThenInclude(b => b.User)
            .Include(c => c.Rooms)
            .AsSplitQuery()
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<Campaign?> GetByIdWithSlotsAsync(Guid id)
    {
        return await Task.FromResult(GetByIdWithSlotsCompiledQuery(_context, id));
    }

    // Новый метод для batch получения мастеров
    public async Task<List<Master>> GetMastersByIdsAsync(List<Guid> masterIds)
    {
        if (!masterIds.Any()) return new List<Master>();
        return await Task.FromResult(GetMastersByIdsCompiledQuery(_context, masterIds).ToList());
    }

    public async Task<List<Campaign>> GetCampaignCatalogAsync()
    {
        return await _dbSet
            .Include(c => c.Tags)
            //.Where(c => c.IsActive)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<List<Campaign>> GetByUserIdAsync(Guid userId)
    {
        return await _dbSet
            .Include(c => c.Masters)
            .Include(c => c.Tags)
            .Include(c => c.Slots)
            .Where(c => c.Masters.Any(m => m.UserId == userId))
            .AsSplitQuery()
            .AsNoTracking()
            .ToListAsync();
    }

    public override async Task<List<Campaign>> GetAllAsync()
    {
        return await _dbSet
            .Include(c => c.Masters)
            .Include(c => c.Tags)
            .Include(c => c.Slots)
            .AsSplitQuery()
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<List<Campaign>> GetActiveCampaignsAsync()
    {
        return await _dbSet
            .Include(c => c.Masters)
            .Include(c => c.Tags)
            .Include(c => c.Slots)
            .Where(c => c.IsActive)
            .AsSplitQuery()
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<List<Campaign>> GetByMasterIdAsync(Guid masterId)
    {
        return await _dbSet
            .Include(c => c.Masters)
            .Include(c => c.Tags)
            .Include(c => c.Slots)
            .Where(c => c.Masters.Any(m => m.Id == masterId))
            .AsSplitQuery()
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<List<Room>> GetRoomsByIdsAsync(List<Guid> roomIds)
    {
        if (!roomIds.Any()) return new List<Room>();
        return await _context.Set<Room>()
            .Where(r => roomIds.Contains(r.Id))
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task UpdateCampaignTagsAsync(Guid campaignId, List<string> tagNames)
    {
        var existingTags = await _context.Set<CampaignTag>()
            .Where(t => t.CampaignId == campaignId)
            .ToListAsync();
        _context.Set<CampaignTag>().RemoveRange(existingTags);
        var newTags = tagNames.Select(name => new CampaignTag(name, campaignId));
        await _context.Set<CampaignTag>().AddRangeAsync(newTags);
    }

    public async Task<List<Room>> GetRoomsByTypesAsync(List<string> roomTypes)
    {
        if (!roomTypes.Any()) return new List<Room>();

        var enumTypes = roomTypes.Select(t => Enum.Parse<RoomType>(t)).ToList();

        return await _context.Set<Room>()
            .Where(r => enumTypes.Contains(r.Type))
            .ToListAsync();
    }

    public async Task UpdateCampaignRoomsAsync(Guid campaignId, List<Guid> roomIds)
    {
        roomIds = roomIds?.Where(id => id != Guid.Empty).ToList() ?? new List<Guid>();

        var campaign = await _dbSet
            .Include(c => c.Rooms)
            .FirstOrDefaultAsync(c => c.Id == campaignId);

        if (campaign == null) return;

        // убрать явно некорректные комнаты в коллекции
        campaign.Rooms.RemoveAll(r => r.Id == Guid.Empty);

        // удалить те, которых нет в новом списке
        campaign.Rooms.RemoveAll(r => !roomIds.Contains(r.Id));

        var existingIds = campaign.Rooms.Select(r => r.Id).ToHashSet();

        foreach (var id in roomIds)
        {
            if (existingIds.Contains(id)) continue;
            var room = await _context.Rooms.FindAsync(id); // tracked or null
            if (room == null) continue;
            _context.Entry(room).State = EntityState.Unchanged;
            campaign.Rooms.Add(room);
        }
    }




    public async Task SaveChangesAsync()
    {

        await _context.SaveChangesAsync();
    }

}