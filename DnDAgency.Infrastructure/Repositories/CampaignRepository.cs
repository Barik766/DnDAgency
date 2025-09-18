using DnDAgency.Domain.Entities;
using DnDAgency.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using ApplicationDbContext = DnDAgency.Infrastructure.Data.ApplicationDbContext;

namespace DnDAgency.Infrastructure.Repositories;

public class CampaignRepository : GenericRepository<Campaign>, ICampaignRepository
{
    public CampaignRepository(ApplicationDbContext context) : base(context) { }

    public async Task<Campaign?> GetByIdForUpdateAsync(Guid id)
    {
        return await _dbSet
            .Include(c => c.Masters)
            .Include(c => c.Tags)
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
            .AsSplitQuery() // Добавлено для оптимизации
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    // НОВЫЙ МЕТОД - только для каталога, без лишних связей
    public async Task<List<Campaign>> GetCampaignCatalogAsync()
    {
        return await _dbSet
            .Include(c => c.Tags) // Только теги
            .Where(c => c.IsActive)
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

    public async Task UpdateCampaignTagsAsync(Guid campaignId, List<string> tagNames)
    {
        var existingTags = await _context.Set<CampaignTag>()
            .Where(t => t.CampaignId == campaignId)
            .ToListAsync();
        _context.Set<CampaignTag>().RemoveRange(existingTags);

        var newTags = tagNames.Select(name => new CampaignTag(name, campaignId));
        await _context.Set<CampaignTag>().AddRangeAsync(newTags);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}