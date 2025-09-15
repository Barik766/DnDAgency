using DnDAgency.Application.Interfaces;
using DnDAgency.Domain.Entities;
using DnDAgency.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;


namespace DnDAgency.Infrastructure.Repositories
{
    public class RefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly ApplicationDbContext _context;
        public RefreshTokenRepository(ApplicationDbContext context) => _context = context;

        public async Task AddAsync(RefreshToken token)
        {
            await _context.Set<RefreshToken>().AddAsync(token);
            await _context.SaveChangesAsync();
        }

        public async Task<RefreshToken?> GetByTokenAsync(string token)
        {
            return await _context.Set<RefreshToken>().FirstOrDefaultAsync(rt => rt.Token == token);
        }

        public async Task UpdateAsync(RefreshToken token)
        {
            _context.Set<RefreshToken>().Update(token);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(RefreshToken token)
        {
            _context.Set<RefreshToken>().Remove(token);
            await _context.SaveChangesAsync();
        }
    }

}
