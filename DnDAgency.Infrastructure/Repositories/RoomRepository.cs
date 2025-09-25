using DnDAgency.Domain.Entities;
using DnDAgency.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using ApplicationDbContext = DnDAgency.Infrastructure.Data.ApplicationDbContext;

namespace DnDAgency.Infrastructure.Repositories
{
    public class RoomRepository : GenericRepository<Room>, IRoomRepository
    {
        public RoomRepository(ApplicationDbContext context) : base(context) { }

        public async Task<List<Room>> GetActiveRoomsAsync()
        {
            return await _dbSet
                .Where(r => r.IsActive)
                .OrderBy(r => r.Name)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Room?> GetByNameAsync(string name)
        {
            return await _dbSet
                .FirstOrDefaultAsync(r => r.Name == name);
        }
    }

    // Дополнительные методы для CampaignRepository
    public partial class CampaignRepository
    {
        public async Task<List<Campaign>> GetByRoomIdAsync(Guid roomId)
        {
            return await _dbSet
                .Include(c => c.Masters)
                .Include(c => c.Slots)
                    .ThenInclude(s => s.Bookings)
                .Where(c => c.RoomId == roomId && c.IsActive)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<List<Campaign>> GetOnlineCampaignsByMasterIdAsync(Guid masterId)
        {
            return await _dbSet
                .Include(c => c.Masters)
                .Include(c => c.Slots)
                    .ThenInclude(s => s.Bookings)
                .Include(c => c.Room)
                .Where(c => c.Masters.Any(m => m.Id == masterId) &&
                           c.Room.Type == RoomType.Online &&
                           c.IsActive)
                .AsNoTracking()
                .ToListAsync();
        }
    }

    // Дополнительный метод для SlotRepository
    public partial class SlotRepository
    {
        public async Task<List<Slot>> GetByCampaignAndDateAsync(Guid campaignId, DateTime date)
        {
            return await _dbSet
                .Include(s => s.Bookings)
                .Where(s => s.CampaignId == campaignId && s.StartTime.Date == date.Date)
                .AsNoTracking()
                .ToListAsync();
        }
    }
}