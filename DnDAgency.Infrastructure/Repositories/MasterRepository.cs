using DnDAgency.Domain.Entities;
using DnDAgency.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using ApplicationDbContext = DnDAgency.Infrastructure.Data.ApplicationDbContext;

namespace DnDAgency.Infrastructure.Repositories;

public class MasterRepository : GenericRepository<Master>, IMasterRepository
{
    private readonly ApplicationDbContext _context;

    public MasterRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<Master?> GetByUserIdAsync(Guid userId)
    {
        return await _dbSet
            .Include(m => m.Campaigns) // если хочешь подгружать связанные кампании
            .FirstOrDefaultAsync(m => m.UserId == userId);
    }

    public override async Task<Master?> GetByIdAsync(Guid id)
    {
        return await _dbSet
            .Include(m => m.Campaigns) // если нужно
            .FirstOrDefaultAsync(m => m.Id == id);
    }
}
