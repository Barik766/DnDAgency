using DnDAgency.Domain.Entities;
using DnDAgency.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using ApplicationDbContext = DnDAgency.Infrastructure.Data.ApplicationDbContext;

namespace DnDAgency.Infrastructure.Repositories;

public class CampaignRepository : GenericRepository<Campaign>, ICampaignRepository
{
    public CampaignRepository(ApplicationDbContext context) : base(context) { }

    public override async Task<Campaign?> GetByIdAsync(Guid id)
    {
        return await _dbSet
            .Include(c => c.Master)
            .Include(c => c.Tags) 
            .Include(c => c.Slots)
                .ThenInclude(s => s.Bookings)
                    .ThenInclude(b => b.User)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public override async Task<List<Campaign>> GetAllAsync()
    {
        return await _dbSet
            .Include(c => c.Master)
            .Include(c => c.Tags)
            .Include(c => c.Slots)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<List<Campaign>> GetActiveCampaignsAsync()
    {
        return await _dbSet
            .Include(c => c.Master)
            .Include(c => c.Tags) 
            .Include(c => c.Slots)
            .Where(c => c.IsActive && c.Slots.Any(s => s.StartTime > DateTime.UtcNow))
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<List<Campaign>> GetByMasterIdAsync(Guid masterId)
    {
        return await _dbSet
            .Include(c => c.Tags)
            .Include(c => c.Slots)
            .Where(c => c.MasterId == masterId)
            .AsNoTracking()
            .ToListAsync();
    }

}