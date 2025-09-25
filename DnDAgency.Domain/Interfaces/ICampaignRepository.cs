using DnDAgency.Domain.Entities;

namespace DnDAgency.Domain.Interfaces;

public interface ICampaignRepository : IRepository<Campaign>
{
    Task SaveChangesAsync();
    Task UpdateCampaignTagsAsync(Guid campaignId, List<string> tagNames);
    Task<Campaign?> GetByIdForUpdateAsync(Guid id);
    Task<List<Campaign>> GetActiveCampaignsAsync();
    Task<List<Campaign>> GetByMasterIdAsync(Guid masterId);
    Task<List<Campaign>> GetByUserIdAsync(Guid userId);
    Task<List<Campaign>> GetCampaignCatalogAsync();
    Task<List<Campaign>> GetByRoomIdAsync(Guid roomId);
    Task<List<Campaign>> GetOnlineCampaignsByMasterIdAsync(Guid masterId);
}