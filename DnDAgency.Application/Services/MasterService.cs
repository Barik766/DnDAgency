using DnDAgency.Application.DTOs.CampaignsDTO;
using DnDAgency.Application.DTOs.MastersDTO;
using DnDAgency.Application.Interfaces;
using DnDAgency.Domain.Entities;
using DnDAgency.Domain.Interfaces;

namespace DnDAgency.Application.Services
{
    public class MasterService : IMasterService
    {
        private readonly IRepository<Master> _masterRepository;
        private readonly IUserRepository _userRepository;
        private readonly ICampaignRepository _campaignRepository;

        public MasterService(
            IRepository<Master> masterRepository,
            IUserRepository userRepository,
            ICampaignRepository campaignRepository)
        {
            _masterRepository = masterRepository;
            _userRepository = userRepository;
            _campaignRepository = campaignRepository;
        }

        public async Task<List<MasterDto>> GetAllAsync()
        {
            var masters = await _masterRepository.GetAllAsync();
            return masters.Where(m => m.IsActive).Select(MapToDto).ToList();
        }

        public async Task<MasterDto> GetByIdAsync(Guid id)
        {
            var master = await _masterRepository.GetByIdAsync(id);
            if (master == null || !master.IsActive)
                throw new KeyNotFoundException("Master not found");

            return MapToDto(master);
        }

        public async Task<MasterDto> CreateFromUserAsync(Guid userId, string bio)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                throw new KeyNotFoundException("User not found");

            if (!user.IsMaster)
                throw new ArgumentException("User must have Master role to create master profile");

            var allMasters = await _masterRepository.GetAllAsync();
            if (allMasters.Any(m => m.UserId == userId))
                throw new ArgumentException("Master profile already exists for this user");

            var master = new Master(userId, user.Username, bio);
            await _masterRepository.AddAsync(master);

            return MapToDto(master);
        }

        public async Task<MasterDto> UpdateAsync(Guid id, UpdateMasterDto dto, Guid currentUserId)
        {
            var master = await _masterRepository.GetByIdAsync(id);
            if (master == null)
                throw new KeyNotFoundException("Master not found");

            if (master.UserId != currentUserId)
                throw new UnauthorizedAccessException("Can only update own master profile");

            if (!string.IsNullOrEmpty(dto.Bio))
                master.UpdateBio(dto.Bio);

            if (!string.IsNullOrEmpty(dto.Name))
                master.UpdateName(dto.Name);

            await _masterRepository.UpdateAsync(master);
            return MapToDto(master);
        }

        public async Task DeactivateAsync(Guid id, Guid currentUserId)
        {
            var master = await _masterRepository.GetByIdAsync(id);
            if (master == null)
                throw new KeyNotFoundException("Master not found");

            if (master.UserId != currentUserId)
                throw new UnauthorizedAccessException("Can only deactivate own master profile");

            master.Deactivate();
            await _masterRepository.UpdateAsync(master);
        }

        public async Task<List<CampaignDto>> GetMasterCampaignsAsync(Guid masterId)
        {
            var campaigns = await _campaignRepository.GetByMasterIdAsync(masterId);
            return campaigns.Select(MapCampaignToDto).ToList();
        }

        private static MasterDto MapToDto(Master master)
        {
            return new MasterDto
            {
                Id = master.Id,
                UserId = master.UserId,
                Name = master.Name,
                Bio = master.Bio,
                IsActive = master.IsActive,
                CampaignCount = master.Campaigns.Count(c => c.IsActive),
                AverageRating = master.Reviews.Any() ? master.Reviews.Average(r => r.Rating) : 0,
                ReviewCount = master.Reviews.Count,
                CreatedAt = master.CreatedAt,
                UpdatedAt = master.UpdatedAt
            };
        }

        private static CampaignDto MapCampaignToDto(Campaign campaign)
        {
            return new CampaignDto
            {
                Id = campaign.Id,
                Title = campaign.Title,
                Description = campaign.Description,
                MasterIds = campaign.Masters?.Select(m => m.Id).ToList() ?? new List<Guid>(),
                MasterNames = campaign.Masters?.Select(m => m.Name).ToList() ?? new List<string>(),
                Price = campaign.Price,
                IsActive = campaign.IsActive,
                HasAvailableSlots = campaign.HasAvailableSlots,
                CreatedAt = campaign.CreatedAt,
                UpdatedAt = campaign.UpdatedAt
            };
        }
    }
}
