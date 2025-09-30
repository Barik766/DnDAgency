using DnDAgency.Application.DTOs.BookingsDTO;
using DnDAgency.Application.DTOs.CampaignsDTO;
using DnDAgency.Domain.Entities;
using DnDAgency.Domain.Interfaces;
using DnDAgency.Infrastructure.Interfaces;

namespace DnDAgency.Application.Services
{
    public class CampaignService : ICampaignService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IFileStorageService _fileStorageService;
        private readonly IConflictCheckService _conflictCheckService;

        public CampaignService(
            IUnitOfWork unitOfWork,
            IFileStorageService fileStorageService,
            IConflictCheckService conflictCheckService)
        {
            _unitOfWork = unitOfWork;
            _fileStorageService = fileStorageService;
            _conflictCheckService = conflictCheckService;
        }

        // --- Получение данных ---
        public async Task<List<CampaignDto>> GetAllAsync()
        {
            var campaigns = await _unitOfWork.Campaigns.GetActiveCampaignsAsync();
            return campaigns.Select(MapToDto).ToList();
        }

        public async Task<CampaignDto> GetByIdAsync(Guid id)
        {
            var campaign = await _unitOfWork.Campaigns.GetByIdAsync(id);
            if (campaign == null)
                throw new KeyNotFoundException("Campaign not found");

            return MapToDto(campaign);
        }

        // --- Создание кампании ---
        public async Task<CampaignDto> CreateAsync(CreateCampaignDto dto, Guid? currentUserId = null, string role = "Master")
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                List<Master> masters = new List<Master>();

                if (role == "Master" && currentUserId.HasValue)
                {
                    var master = await _unitOfWork.Masters.GetByIdAsync(currentUserId.Value);
                    if (master == null)
                        throw new KeyNotFoundException("Master not found");
                    masters.Add(master);
                }
                else if (role == "Admin" && dto.MasterIds.Any())
                {
                    masters = await _unitOfWork.Campaigns.GetMastersByIdsAsync(dto.MasterIds);

                    if (masters.Count != dto.MasterIds.Count)
                    {
                        var foundIds = masters.Select(m => m.Id).ToHashSet();
                        var notFound = dto.MasterIds.Where(id => !foundIds.Contains(id)).ToList();
                        throw new KeyNotFoundException($"Masters not found: {string.Join(", ", notFound)}");
                    }
                }

                var rooms = await _unitOfWork.Campaigns.GetRoomsByIdsAsync(dto.RoomIds);
                if (rooms.Count != dto.RoomIds.Count)
                {
                    var foundIds = rooms.Select(r => r.Id).ToHashSet();
                    var notFound = dto.RoomIds.Where(id => !foundIds.Contains(id)).ToList();
                    throw new KeyNotFoundException($"Rooms not found: {string.Join(", ", notFound)}");
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
                    rooms,
                    dto.MaxPlayers,
                    dto.DurationHours,
                    masters
                );

                if (dto.Tags != null)
                    campaign.Tags.AddRange(dto.Tags.Select(tag => new CampaignTag(tag, campaign.Id)));

                await _unitOfWork.Campaigns.AddAsync(campaign);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                return MapToDto(campaign);
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
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
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var campaign = await _unitOfWork.Campaigns.GetByIdForUpdateAsync(id);
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

                // Обработка тегов
                if (dto.Tags != null)
                {
                    await _unitOfWork.Campaigns.UpdateCampaignTagsAsync(campaign.Id, dto.Tags);
                }

                // Обработка мастеров (только для админа)
                if (role == "Admin" && dto.MasterIds != null)
                {
                    campaign.Masters.Clear();

                    if (dto.MasterIds.Any())
                    {
                        var newMasters = await _unitOfWork.Campaigns.GetMastersByIdsAsync(dto.MasterIds);

                        if (newMasters.Count != dto.MasterIds.Count)
                        {
                            var foundIds = newMasters.Select(m => m.Id).ToHashSet();
                            var notFound = dto.MasterIds.Where(id => !foundIds.Contains(id)).ToList();
                            throw new KeyNotFoundException($"Masters not found: {string.Join(", ", notFound)}");
                        }

                        foreach (var master in newMasters)
                            campaign.Masters.Add(master);
                    }
                }

                // Обработка комнат
                if (dto.SupportedRoomTypes != null)
                {
                    var roomIds = dto.SupportedRoomTypes.Any()
                        ? (await _unitOfWork.Campaigns.GetRoomsByTypesAsync(dto.SupportedRoomTypes))
                              .Select(r => r.Id).ToList()
                        : new List<Guid>();

                    await _unitOfWork.Campaigns.UpdateCampaignRoomsAsync(campaign.Id, roomIds);
                }

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();
                return MapToDto(campaign);
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        // --- Удаление кампании ---
        public async Task DeleteAsync(Guid id, Guid currentUserId, string role, Guid? masterUserId = null)
        {
            var campaign = await _unitOfWork.Campaigns.GetByIdForUpdateAsync(id);
            if (campaign == null)
                throw new KeyNotFoundException("Campaign not found");

            CheckAccess(campaign, currentUserId, role);

            var slots = await _unitOfWork.Slots.GetByCampaignIdAsync(id);
            var hasActiveBookings = slots.Any(s => s.Bookings.Any() && s.StartTime > DateTime.UtcNow);

            if (hasActiveBookings)
                throw new InvalidOperationException("Cannot delete campaign with active bookings");

            campaign.Deactivate();
            await _unitOfWork.SaveChangesAsync();
        }

        // --- Переключение активности ---
        public async Task<CampaignDto> ToggleStatusAsync(Guid id, Guid currentUserId, string role, Guid? masterUserId = null)
        {
            var campaign = await _unitOfWork.Campaigns.GetByIdForUpdateAsync(id);
            if (campaign == null)
                throw new KeyNotFoundException("Campaign not found");

            CheckAccess(campaign, currentUserId, role);

            if (campaign.IsActive)
                campaign.Deactivate();
            else
                campaign.Activate();

            await _unitOfWork.SaveChangesAsync();
            return MapToDto(campaign);
        }

        // --- Детали и каталог ---
        public async Task<CampaignDetailsDto> GetCampaignDetailsAsync(Guid id)
        {
            var campaign = await _unitOfWork.Campaigns.GetByIdAsync(id);
            if (campaign == null)
                throw new KeyNotFoundException("Campaign not found");

            return MapToCampaignDetailsDto(campaign);
        }

        public async Task<List<CampaignCatalogDto>> GetCampaignCatalogAsync()
        {
            var campaigns = await _unitOfWork.Campaigns.GetCampaignCatalogAsync();

            var campaignIds = campaigns.Select(c => c.Id).ToList();
            var campaignIdsWithSlots = await _unitOfWork.Slots.GetCampaignIdsWithAvailableSlotsAsync(campaignIds);

            return campaigns.Select(c => MapToCampaignCatalogDto(c, campaignIdsWithSlots.Contains(c.Id))).ToList();
        }

        public async Task<List<UpcomingGameDto>> GetUpcomingGamesAsync()
        {
            var slots = await _unitOfWork.Slots.GetUpcomingSlotsAsync();
            return slots.Select(MapToUpcomingGameDto).ToList();
        }

        public async Task<List<AvailableTimeSlot>> GetAvailableTimeSlotsAsync(Guid campaignId, DateTime date, RoomType roomType)
        {
            var campaign = await _unitOfWork.Campaigns.GetByIdWithSlotsAsync(campaignId);
            if (campaign == null || !campaign.DurationHours.HasValue || !campaign.SupportsRoomType(roomType))
                return new List<AvailableTimeSlot>();

            var room = campaign.Rooms.FirstOrDefault(r => r.Type == roomType);
            if (room == null)
                return new List<AvailableTimeSlot>();

            var roomConflicts = await _unitOfWork.Slots.GetBookedSlotsForRoomAndDateAsync(room.Id, date);
            var existingSlots = campaign.Slots.Where(s => s.StartTime.Date == date.Date).ToList();

            var slotIds = existingSlots.Select(s => s.Id).ToList();
            var playersCount = slotIds.Any()
                ? await _unitOfWork.Slots.GetPlayersCountForSlotsAsync(slotIds)
                : new Dictionary<Guid, int>();

            var duration = TimeSpan.FromHours(campaign.DurationHours.Value);
            var result = new List<AvailableTimeSlot>();
            var currentTime = campaign.WorkingHoursStart;

            while (currentTime <= campaign.GetMaxStartTime())
            {
                var slotDateTime = DateTime.SpecifyKind(date.Date, DateTimeKind.Utc).Add(currentTime);
                if (slotDateTime <= DateTime.UtcNow)
                {
                    currentTime = currentTime.Add(TimeSpan.FromHours(1));
                    continue;
                }

                var existingSlot = existingSlots.FirstOrDefault(s => s.StartTime == slotDateTime);
                var currentPlayers = existingSlot != null ? playersCount.GetValueOrDefault(existingSlot.Id, 0) : 0;
                bool isAvailable = currentPlayers < campaign.MaxPlayers;

                if (isAvailable && HasTimeConflict(slotDateTime, duration, roomConflicts, existingSlot?.Id))
                {
                    isAvailable = false;
                }

                if (isAvailable)
                {
                    result.Add(new AvailableTimeSlot
                    {
                        DateTime = slotDateTime,
                        IsAvailable = true,
                        CurrentPlayers = currentPlayers,
                        MaxPlayers = campaign.MaxPlayers,
                        RoomType = roomType
                    });
                }
                currentTime = currentTime.Add(TimeSpan.FromHours(1));
            }
            return result;
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

        private static CampaignCatalogDto MapToCampaignCatalogDto(Campaign campaign, bool hasAvailableSlots)
        {
            return new CampaignCatalogDto
            {
                Id = campaign.Id,
                Title = campaign.Title,
                ImageUrl = campaign.ImageUrl,
                Level = campaign.Level,
                Price = campaign.Price,
                Tags = campaign.Tags.Select(t => t.Name).ToList(),
                HasAvailableSlots = hasAvailableSlots,
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
                CampaignImageUrl = campaign.ImageUrl, // Добавь если есть
                Level = campaign.Level,
                StartTime = slot.StartTime,
                MaxPlayers = campaign.MaxPlayers, // Добавь эту строку
                BookedPlayers = slot.CurrentPlayers,
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
                UpdatedAt = campaign.UpdatedAt ?? campaign.CreatedAt,
                SupportedRoomTypes = campaign.Rooms.Select(r => r.Type.ToString()).Distinct().ToList()
            };
        }

        private static SlotDto MapSlotToDto(Slot slot)
        {
            var campaign = slot.Campaign ?? throw new InvalidOperationException("Slot must have a campaign reference");
            var currentPlayers = slot.Bookings.Count;
            var availableSlots = campaign.MaxPlayers - currentPlayers;

            var roomType = campaign.Rooms.FirstOrDefault()?.Type ?? RoomType.Physical;

            return new SlotDto
            {
                Id = slot.Id,
                CampaignId = slot.CampaignId,
                StartTime = slot.StartTime,
                CurrentPlayers = currentPlayers,
                AvailableSlots = availableSlots,
                IsFull = availableSlots <= 0,
                IsInPast = slot.StartTime < DateTime.UtcNow,
                RoomType = roomType
            };
        }

        // --- Вспомогательные методы ---
        private bool HasTimeConflict(DateTime startTime, TimeSpan duration, List<ConflictSlot> conflicts, Guid? excludeSlotId = null)
        {
            var endTime = startTime.Add(duration);

            return conflicts.Any(conflict =>
                conflict.Id != excludeSlotId &&
                ConflictCheckService.TimeIntervalsOverlap(startTime, endTime, conflict.StartTime, conflict.EndTime)
            );
        }
    }
}