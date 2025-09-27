using DnDAgency.Domain.Entities;
using DnDAgency.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DnDAgency.Application.Interfaces
{
    public interface IRefreshTokenRepository : IRepository<RefreshToken>
    {
        Task<RefreshToken?> GetByTokenAsync(string token);
    }

}
