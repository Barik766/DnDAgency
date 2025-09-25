using DnDAgency.Application.DTOs.BookingsDTO;
using DnDAgency.Application.DTOs.CampaignsDTO;

public interface ICampaignService
{
    Task<List<CampaignDto>> GetAllAsync();
    Task<CampaignDto> GetByIdAsync(Guid id);
    Task<CampaignDto> CreateAsync(CreateCampaignDto dto, Guid? currentUserId = null, string role = "Master");
    Task<CampaignDto> UpdateAsync(Guid id, UpdateCampaignDto dto, Guid currentUserId, string role, Guid? masterUserId = null);
    Task DeleteAsync(Guid id, Guid currentUserId, string role, Guid? masterUserId = null);
    Task<CampaignDto> ToggleStatusAsync(Guid id, Guid currentUserId, string role, Guid? masterUserId = null);

    // Новый метод для получения доступных временных слотов
    Task<List<AvailableTimeSlot>> GetAvailableTimeSlotsAsync(Guid campaignId, DateTime date, RoomType roomType);

    // Методы для отображения данных
    Task<CampaignDetailsDto> GetCampaignDetailsAsync(Guid id);
    Task<List<CampaignCatalogDto>> GetCampaignCatalogAsync();
    Task<List<UpcomingGameDto>> GetUpcomingGamesAsync();

}