using DnDAgency.Application.DTOs.BookingsDTO;
using DnDAgency.Application.DTOs.CampaignsDTO;

public interface ICampaignService
{
    Task<List<CampaignDto>> GetAllAsync();
    Task<CampaignDto> GetByIdAsync(Guid id);

    // masterId больше не обязателен, админ может создавать без мастера
    Task<CampaignDto> CreateAsync(CreateCampaignDto dto, Guid? currentUserId = null, string role = "Master");

    Task<CampaignDto> UpdateAsync(Guid id, UpdateCampaignDto dto, Guid currentUserId, string role, Guid? masterUserId = null);
    
    Task DeleteAsync(Guid id, Guid currentUserId, string role, Guid? masterUserId = null);
    Task<CampaignDto> ToggleStatusAsync(Guid id, Guid currentUserId, string role, Guid? masterUserId = null);

    Task<List<SlotDto>> GetCampaignSlotsAsync(Guid campaignId);
    Task<SlotDto> AddSlotToCampaignAsync(Guid campaignId, DateTime startTime, Guid currentUserId, string role, Guid? masterUserId = null);
    Task RemoveSlotFromCampaignAsync(Guid campaignId, Guid slotId, Guid currentUserId, string role, Guid? masterUserId = null);

    Task<CampaignDetailsDto> GetCampaignDetailsAsync(Guid id);
    Task<List<CampaignCatalogDto>> GetCampaignCatalogAsync();
    Task<List<UpcomingGameDto>> GetUpcomingGamesAsync();

}
