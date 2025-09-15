using DnDAgency.Domain.Entities;

namespace DnDAgency.Domain.Interfaces;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(string email);

}
