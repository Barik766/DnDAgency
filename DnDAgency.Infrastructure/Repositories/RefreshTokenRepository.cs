using DnDAgency.Application.Interfaces;
using DnDAgency.Domain.Entities;
using DnDAgency.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DnDAgency.Infrastructure.Repositories
{
    public class RefreshTokenRepository : GenericRepository<RefreshToken>, IRefreshTokenRepository
    {
        public RefreshTokenRepository(ApplicationDbContext context) : base(context) { }

        public async Task<RefreshToken?> GetByTokenAsync(string token)
        {
            return await _dbSet.FirstOrDefaultAsync(rt => rt.Token == token);
        }

        
    }
}