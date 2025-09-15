using DnDAgency.Domain.Entities;

namespace DnDAgency.Domain.Interfaces;

public interface ICampaignRepository : IRepository<Campaign>
{
    Task<List<Campaign>> GetActiveCampaignsAsync();
    Task<List<Campaign>> GetByMasterIdAsync(Guid masterId);
}