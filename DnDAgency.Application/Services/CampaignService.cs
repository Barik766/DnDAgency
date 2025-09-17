using DnDAgency.Application.DTOs.BookingsDTO;
using DnDAgency.Application.DTOs.CampaignsDTO;
using DnDAgency.Domain.Entities;
using DnDAgency.Domain.Interfaces;
using DnDAgency.Infrastructure.Interfaces;

namespace DnDAgency.Application.Services
{
    public class CampaignService : ICampaignService
    {
        private readonly ICampaignRepository _campaignRepository;
        private readonly ISlotRepository _slotRepository;
        private readonly IMasterRepository _masterRepository;
        private readonly IFileStorageService _fileStorageService;

        public CampaignService(
            ICampaignRepository campaignRepository,
            ISlotRepository slotRepository,
            IMasterRepository masterRepository,
            IFileStorageService fileStorageService)
        {
            _campaignRepository = campaignRepository;
            _slotRepository = slotRepository;
            _masterRepository = masterRepository;
            _fileStorageService = fileStorageService;
        }

        // --- Получение данных ---
        public async Task<List<CampaignDto>> GetAllAsync()
        {
            var campaigns = await _campaignRepository.GetActiveCampaignsAsync();
            return campaigns.Select(MapToDto).ToList();
        }

        public async Task<CampaignDto> GetByIdAsync(Guid id)
        {
            var campaign = await _campaignRepository.GetByIdAsync(id);
            if (campaign == null)
                throw new KeyNotFoundException("Campaign not found");

            return MapToDto(campaign);
        }

        // --- Создание кампании ---
        public async Task<CampaignDto> CreateAsync(CreateCampaignDto dto, Guid? currentUserId = null, string role = "Master")
        {
            List<Master> masters = new List<Master>();

            if (role == "Master" && currentUserId.HasValue)
            {
                var master = await _masterRepository.GetByIdAsync(currentUserId.Value);
                if (master == null)
                    throw new KeyNotFoundException("Master not found");

                masters.Add(master);
            }
            else if (role == "Admin" && dto.MasterIds.Any())
            {
                foreach (var masterId in dto.MasterIds)
                {
                    var master = await _masterRepository.GetByIdAsync(masterId);
                    if (master != null)
                        masters.Add(master);
                }
            }

            string? imageUrl = null;
            if (dto.ImageFile != null)
                imageUrl = await _fileStorageService.SaveFileAsync(dto.ImageFile, "campaigns");

            var campaign = new Campaign(
                dto.Title,
                dto.Description,
                dto.Price,
                imageUrl,
                dto.Level,
                dto.MaxPlayers,
                dto.DurationHours,
                masters
            );

            if (dto.Tags != null)
                campaign.Tags.AddRange(dto.Tags.Select(tag => new CampaignTag(tag, campaign.Id)));

            await _campaignRepository.AddAsync(campaign);
            return MapToDto(campaign);
        }

        // --- Проверка прав доступа ---
        private static void CheckAccess(Campaign campaign, Guid currentUserId, string role)
        {
            if (role == "Admin")
                return;

            if (role == "Master" && (campaign.Masters == null || !campaign.Masters.Any(m => m.Id == currentUserId)))
                throw new UnauthorizedAccessException("You can only modify your own campaigns");

            if (role != "Admin" && role != "Master")
                throw new UnauthorizedAccessException("Invalid role");
        }

        // --- Обновление кампании ---
        public async Task<CampaignDto> UpdateAsync(Guid id, UpdateCampaignDto dto, Guid currentUserId, string role, Guid? masterUserId = null)
        {
            // Получаем сущность БЕЗ AsNoTracking, чтобы EF мог её отслеживать
            var campaign = await _campaignRepository.GetByIdForUpdateAsync(id);
            if (campaign == null)
                throw new KeyNotFoundException("Campaign not found");

            CheckAccess(campaign, currentUserId, role);

            // Обновляем поля через методы домена
            if (!string.IsNullOrWhiteSpace(dto.Title))
                campaign.UpdateTitle(dto.Title);
            if (!string.IsNullOrWhiteSpace(dto.Description))
                campaign.UpdateDescription(dto.Description);
            if (dto.Price.HasValue)
                campaign.UpdatePrice(dto.Price.Value);
            if (dto.Level.HasValue)
                campaign.UpdateLevel(dto.Level.Value);
            if (dto.MaxPlayers.HasValue)
                campaign.UpdateMaxPlayers(dto.MaxPlayers.Value);
            if (dto.DurationHours.HasValue)
                campaign.UpdateDuration(dto.DurationHours);

            // Обработка изображения
            if (dto.ImageFile != null)
            {
                var imageUrl = await _fileStorageService.SaveFileAsync(dto.ImageFile, "campaigns");
                campaign.UpdateImageUrl(imageUrl);
            }

            // Обработка тегов - правильный способ для EF
            if (dto.Tags != null)
            {
                await _campaignRepository.UpdateCampaignTagsAsync(campaign.Id, dto.Tags);
            }

            // Обработка мастеров (только для админа)
            if (role == "Admin" && dto.MasterIds != null)
            {
                // Очищаем список мастеров
                campaign.Masters.Clear();

                // Добавляем новых мастеров
                foreach (var masterId in dto.MasterIds)
                {
                    var master = await _masterRepository.GetByIdAsync(masterId);
                    if (master != null)
                        campaign.Masters.Add(master);
                }
            }

            // Сохраняем изменения ОДИН раз
            await _campaignRepository.SaveChangesAsync();

            return MapToDto(campaign);
        }


        // --- Удаление кампании ---
        public async Task DeleteAsync(Guid id, Guid currentUserId, string role, Guid? masterUserId = null)
        {
            var campaign = await _campaignRepository.GetByIdAsync(id);
            if (campaign == null)
                throw new KeyNotFoundException("Campaign not found");

            CheckAccess(campaign, currentUserId, role);

            var slots = await _slotRepository.GetByCampaignIdAsync(id);
            var hasActiveBookings = slots.Any(s => s.Bookings.Any() && s.StartTime > DateTime.UtcNow);

            if (hasActiveBookings)
                throw new InvalidOperationException("Cannot delete campaign with active bookings");

            campaign.Deactivate();
            await _campaignRepository.UpdateAsync(campaign);
        }

        // --- Переключение активности ---
        public async Task<CampaignDto> ToggleStatusAsync(Guid id, Guid currentUserId, string role, Guid? masterUserId = null)
        {
            var campaign = await _campaignRepository.GetByIdAsync(id);
            if (campaign == null)
                throw new KeyNotFoundException("Campaign not found");

            CheckAccess(campaign, currentUserId, role);

            if (campaign.IsActive)
                campaign.Deactivate();
            else
                campaign.Activate();

            await _campaignRepository.UpdateAsync(campaign);
            return MapToDto(campaign);
        }

        // --- Слоты ---
        public async Task<List<SlotDto>> GetCampaignSlotsAsync(Guid campaignId)
        {
            var slots = await _slotRepository.GetByCampaignIdAsync(campaignId);
            return slots.Select(MapSlotToDto).ToList();
        }

        public async Task<SlotDto> AddSlotToCampaignAsync(Guid campaignId, DateTime startTime, Guid currentUserId, string role, Guid? masterUserId = null)
        {
            var campaign = await _campaignRepository.GetByIdAsync(campaignId);
            if (campaign == null)
                throw new KeyNotFoundException("Campaign not found");

            CheckAccess(campaign, currentUserId, role);

            if (startTime <= DateTime.UtcNow)
                throw new ArgumentException("Start time must be in the future");

            var slot = new Slot(campaignId, startTime);
            await _slotRepository.AddAsync(slot);
            return MapSlotToDto(slot);
        }

        public async Task RemoveSlotFromCampaignAsync(Guid campaignId, Guid slotId, Guid currentUserId, string role, Guid? masterUserId = null)
        {
            var campaign = await _campaignRepository.GetByIdAsync(campaignId);
            if (campaign == null)
                throw new KeyNotFoundException("Campaign not found");

            CheckAccess(campaign, currentUserId, role);

            var slot = await _slotRepository.GetByIdAsync(slotId);
            if (slot == null || slot.CampaignId != campaignId)
                throw new KeyNotFoundException("Slot not found in this campaign");

            if (slot.Bookings.Any())
                throw new InvalidOperationException("Cannot remove slot with existing bookings");

            await _slotRepository.DeleteAsync(slot);
        }

        // --- Детали и каталог ---
        public async Task<CampaignDetailsDto> GetCampaignDetailsAsync(Guid id)
        {
            var campaign = await _campaignRepository.GetByIdAsync(id);
            if (campaign == null)
                throw new KeyNotFoundException("Campaign not found");

            return MapToCampaignDetailsDto(campaign);
        }

        public async Task<List<CampaignCatalogDto>> GetCampaignCatalogAsync()
        {
            var campaigns = await _campaignRepository.GetActiveCampaignsAsync();
            return campaigns.Select(MapToCampaignCatalogDto).ToList();
        }

        public async Task<List<UpcomingGameDto>> GetUpcomingGamesAsync()
        {
            var slots = await _slotRepository.GetUpcomingSlotsAsync();
            return slots.Select(MapToUpcomingGameDto).ToList();
        }

        // --- Mapping ---
        private static CampaignDetailsDto MapToCampaignDetailsDto(Campaign campaign)
        {
            return new CampaignDetailsDto
            {
                Id = campaign.Id,
                Title = campaign.Title,
                Description = campaign.Description,
                MasterName = string.Join(", ", campaign.Masters?.Select(m => m.Name) ?? new List<string> { "Unknown" }),
                Price = campaign.Price,
                ImageUrl = campaign.ImageUrl,
                Level = campaign.Level,
                MaxPlayers = campaign.MaxPlayers,
                DurationHours = campaign.DurationHours,
                Tags = campaign.Tags.Select(t => t.Name).ToList(),
                IsActive = campaign.IsActive,
                Slots = campaign.Slots.Select(MapSlotToDto).ToList()
            };
        }

        private static CampaignCatalogDto MapToCampaignCatalogDto(Campaign campaign)
        {
            return new CampaignCatalogDto
            {
                Id = campaign.Id,
                Title = campaign.Title,
                ImageUrl = campaign.ImageUrl,
                Level = campaign.Level,
                Price = campaign.Price,
                Tags = campaign.Tags.Select(t => t.Name).ToList(),
                HasAvailableSlots = campaign.HasAvailableSlots,
                IsActive = campaign.IsActive
            };
        }

        private static UpcomingGameDto MapToUpcomingGameDto(Slot slot)
        {
            var campaign = slot.Campaign ?? throw new InvalidOperationException("Slot must have a campaign reference");

            return new UpcomingGameDto
            {
                SlotId = slot.Id,
                CampaignId = campaign.Id,
                CampaignTitle = campaign.Title,
                Level = campaign.Level,
                StartTime = slot.StartTime,
                BookedPlayers = slot.Bookings.Count,
                AvailableSlots = campaign.MaxPlayers - slot.Bookings.Count,
                IsFull = (campaign.MaxPlayers - slot.Bookings.Count) <= 0
            };
        }

        private static CampaignDto MapToDto(Campaign campaign)
        {
            return new CampaignDto
            {
                Id = campaign.Id,
                Title = campaign.Title,
                Description = campaign.Description,
                MasterIds = campaign.Masters?.Select(m => m.Id).ToList() ?? new List<Guid>(),
                MasterNames = campaign.Masters?.Select(m => m.Name).ToList() ?? new List<string>(),
                Price = campaign.Price,
                ImageUrl = campaign.ImageUrl,
                Level = campaign.Level,
                MaxPlayers = campaign.MaxPlayers,
                Duration = campaign.DurationHours.HasValue ? TimeSpan.FromHours(campaign.DurationHours.Value) : (TimeSpan?)null,
                Tags = campaign.Tags.Select(t => t.Name).ToList(),
                IsActive = campaign.IsActive,
                HasAvailableSlots = campaign.HasAvailableSlots,
                CreatedAt = campaign.CreatedAt,
                UpdatedAt = campaign.UpdatedAt ?? campaign.CreatedAt
            };
        }

        private static SlotDto MapSlotToDto(Slot slot)
        {
            var campaign = slot.Campaign ?? throw new InvalidOperationException("Slot must have a campaign reference");
            var currentPlayers = slot.Bookings.Count;
            var availableSlots = campaign.MaxPlayers - currentPlayers;

            return new SlotDto
            {
                Id = slot.Id,
                CampaignId = slot.CampaignId,
                StartTime = slot.StartTime,
                CurrentPlayers = currentPlayers,
                AvailableSlots = availableSlots,
                IsFull = availableSlots <= 0,
                IsInPast = slot.StartTime < DateTime.UtcNow
            };
        }
    }
}
