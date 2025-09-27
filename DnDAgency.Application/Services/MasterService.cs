using DnDAgency.Application.DTOs.CampaignsDTO;
using DnDAgency.Application.DTOs.MastersDTO;
using DnDAgency.Application.DTOs.UsersDTO;
using DnDAgency.Application.Interfaces;
using DnDAgency.Domain.Entities;
using DnDAgency.Infrastructure.Interfaces;

namespace DnDAgency.Application.Services
{
    public class MasterService : IMasterService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IFileStorageService _fileStorageService;
        private readonly IUserService _userService;

        public MasterService(
            IUnitOfWork unitOfWork,
            IFileStorageService fileStorageService,
            IUserService userService)
        {
            _unitOfWork = unitOfWork;
            _fileStorageService = fileStorageService;
            _userService = userService;
        }

        public async Task<List<MasterDto>> GetAllAsync()
        {
            var masters = await _unitOfWork.Masters.GetAllAsync();
            return masters.Where(m => m.IsActive).Select(MapToDto).ToList();
        }

        public async Task<MasterDto> GetByIdAsync(Guid id)
        {
            var master = await _unitOfWork.Masters.GetByIdAsync(id);
            if (master == null || !master.IsActive)
                throw new KeyNotFoundException("Master not found");

            return MapToDto(master);
        }

        public async Task<MasterDto> CreateFromUserAsync(Guid userId, string bio)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var user = await _unitOfWork.Users.GetByIdAsync(userId);
                if (user == null)
                    throw new KeyNotFoundException("User not found");

                if (!user.IsMaster)
                    throw new ArgumentException("User must have Master role to create master profile");

                var allMasters = await _unitOfWork.Masters.GetAllAsync();
                if (allMasters.Any(m => m.UserId == userId))
                    throw new ArgumentException("Master profile already exists for this user");

                var master = new Master(userId, user.Username, bio);
                await _unitOfWork.Masters.AddAsync(master);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                return MapToDto(master);
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<MasterDto> AdminCreateMasterAsync(AdminCreateMasterDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                // 1. Создаём нового пользователя через IUserService
                var userDto = await _userService.CreateAsync(dto.Username, dto.Email, dto.Password);

                // 2. Получаем сущность пользователя
                var userEntity = await _unitOfWork.Users.GetByIdAsync(userDto.Id);
                if (userEntity == null)
                    throw new Exception("Failed to create user for master");

                // 3. Ставим роль Master и создаём профиль мастера
                userEntity.PromoteToMaster();
                var master = userEntity.CreateMasterProfile(dto.Bio);

                // 4. Сохраняем фото, если есть
                if (dto.Photo != null)
                {
                    var photoUrl = await _fileStorageService.SaveFileAsync(dto.Photo, "masters");
                    typeof(Master).GetProperty("PhotoUrl")?.SetValue(master, photoUrl);
                }

                // 5. Сохраняем мастер-профиль в репозитории
                await _unitOfWork.Masters.AddAsync(master);
                _unitOfWork.Users.Update(userEntity);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                return MapAdminDto(master);
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<MasterDto> UpdateAsync(Guid id, UpdateMasterDto dto, Guid currentUserId)
        {
            var master = await _unitOfWork.Masters.GetByIdAsync(id);
            if (master == null)
                throw new KeyNotFoundException("Master not found");

            if (master.UserId != currentUserId)
                throw new UnauthorizedAccessException("Can only update own master profile");

            if (!string.IsNullOrEmpty(dto.Bio))
                master.UpdateBio(dto.Bio);

            if (!string.IsNullOrEmpty(dto.Name))
                master.UpdateName(dto.Name);

            _unitOfWork.Masters.Update(master);
            await _unitOfWork.SaveChangesAsync();
            return MapToDto(master);
        }

        public async Task DeactivateAsync(Guid id, Guid currentUserId)
        {
            var master = await _unitOfWork.Masters.GetByIdAsync(id);
            if (master == null)
                throw new KeyNotFoundException("Master not found");

            if (master.UserId != currentUserId)
                throw new UnauthorizedAccessException("Can only deactivate own master profile");

            master.Deactivate();
            _unitOfWork.Masters.Update(master);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<List<CampaignDto>> GetMasterCampaignsAsync(Guid userId)
        {
            var campaigns = await _unitOfWork.Campaigns.GetByUserIdAsync(userId);
            return campaigns.Where(c => c.IsActive).Select(MapCampaignToDto).ToList();
        }

        public async Task AddCampaignToMasterAsync(Guid masterId, Guid campaignId)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var master = await _unitOfWork.Masters.GetByIdAsync(masterId);
                if (master == null || !master.IsActive)
                    throw new KeyNotFoundException("Master not found");

                var campaign = await _unitOfWork.Campaigns.GetByIdForUpdateAsync(campaignId);
                if (campaign == null || !campaign.IsActive)
                    throw new KeyNotFoundException("Campaign not found");

                // Проверка: не добавлена ли уже эта кампания
                if (campaign.Masters.Any(m => m.Id == master.Id))
                    throw new InvalidOperationException("Campaign is already assigned to this master");

                campaign.Masters.Add(master);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task AssignCampaignsAsync(Guid userId, List<Guid> campaignIds, Guid currentUserId)
        {
            if (userId != currentUserId)
                throw new UnauthorizedAccessException("You can only assign campaigns to your own profile");

            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                // Получаем мастера с кампаниями для отслеживания
                var master = await _unitOfWork.Masters.GetByUserIdAsync(userId);
                if (master == null || !master.IsActive)
                    throw new KeyNotFoundException("Master not found");

                // Очищаем текущие связи
                master.Campaigns.Clear();

                // Добавляем только выбранные кампании
                foreach (var campaignId in campaignIds)
                {
                    var campaign = await _unitOfWork.Campaigns.GetByIdForUpdateAsync(campaignId);
                    if (campaign == null || !campaign.IsActive)
                        continue; // пропускаем несуществующие

                    master.Campaigns.Add(campaign);
                }

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        // --- Мапперы ---
        private static MasterDto MapToDto(Master master)
        {
            return new MasterDto
            {
                Id = master.Id,
                UserId = master.UserId,
                Name = master.Name,
                Bio = master.Bio,
                PhotoUrl = master.PhotoUrl,
                IsActive = master.IsActive,
                CampaignCount = master.Campaigns.Count(c => c.IsActive),
                AverageRating = master.Reviews.Any() ? master.Reviews.Average(r => r.Rating) : 0,
                ReviewCount = master.Reviews.Count,
                CreatedAt = master.CreatedAt,
                UpdatedAt = master.UpdatedAt
            };
        }

        private static MasterDto MapAdminDto(Master master)
        {
            return new MasterDto
            {
                Id = master.Id,
                Name = master.Name,
                Bio = master.Bio,
                PhotoUrl = master.PhotoUrl,
                IsActive = master.IsActive,
                CampaignCount = 0,
                AverageRating = 0,
                ReviewCount = 0,
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