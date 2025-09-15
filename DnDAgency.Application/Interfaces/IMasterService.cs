using DnDAgency.Application.DTOs.CampaignsDTO;
using DnDAgency.Application.DTOs.MastersDTO;

namespace DnDAgency.Application.Interfaces
{
    public interface IMasterService
    {
        Task<List<MasterDto>> GetAllAsync();
        Task<MasterDto> GetByIdAsync(Guid id);
        Task<MasterDto> CreateFromUserAsync(Guid userId, string bio);
        Task<MasterDto> UpdateAsync(Guid id, UpdateMasterDto dto, Guid currentUserId);
        Task DeactivateAsync(Guid id, Guid currentUserId);
        Task<List<CampaignDto>> GetMasterCampaignsAsync(Guid masterId);
    }
}