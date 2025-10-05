using DnDAgency.Domain.Entities;

namespace DnDAgency.Domain.Interfaces;

public interface ICampaignRepository : IRepository<Campaign>
{
    Task UpdateCampaignTagsAsync(Guid campaignId, List<string> tagNames);
    Task<Campaign?> GetByIdForUpdateAsync(Guid id);
    Task<List<Campaign>> GetActiveCampaignsAsync();
    Task<List<Campaign>> GetByMasterIdAsync(Guid masterId);
    Task<List<Campaign>> GetByUserIdAsync(Guid userId);
    Task<List<Campaign>> GetCampaignCatalogAsync();
    Task<(List<Campaign> Campaigns, int TotalCount)> GetCampaignCatalogPagedAsync(int pageNumber, int pageSize);
    Task<(List<Campaign> Campaigns, int TotalCount)> GetCampaignCatalogFilteredAsync(
        string? search,
        string? tag,
        bool? hasSlots,
        string sortBy,
        int pageNumber,
        int pageSize);
    Task<List<Campaign>> GetByRoomIdAsync(Guid roomId);
    Task<List<Campaign>> GetOnlineCampaignsByMasterIdAsync(Guid masterId);
    Task<Campaign?> GetByIdWithSlotsAsync(Guid id);
    Task<List<Master>> GetMastersByIdsAsync(List<Guid> masterIds);
    Task<List<Room>> GetRoomsByIdsAsync(List<Guid> roomIds);
    Task<List<Room>> GetRoomsByTypesAsync(List<string> roomTypes);
    Task UpdateCampaignRoomsAsync(Guid campaignId, List<Guid> roomIds);
}