using DnDAgency.Domain.Entities;
using DnDAgency.Domain.Interfaces;


namespace DnDAgency.Domain.Interfaces
{
    public interface IRoomRepository : IRepository<Room>
    {
        Task<List<Room>> GetActiveRoomsAsync();
        Task<Room?> GetByNameAsync(string name);
    }
}
