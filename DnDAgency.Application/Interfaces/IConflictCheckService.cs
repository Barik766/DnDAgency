using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DnDAgency.Domain.Interfaces
{
    public interface IConflictCheckService
    {
        Task<bool> HasConflictAsync(Guid campaignId, DateTime startTime, TimeSpan duration);
    }
}
