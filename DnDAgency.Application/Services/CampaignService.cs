using DnDAgency.Application.DTOs.BookingsDTO;
using DnDAgency.Application.DTOs.CampaignsDTO;
using DnDAgency.Application.Interfaces;
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
        private readonly ICacheService _cacheService;

        public CampaignService(
            IUnitOfWork unitOfWork,
            IFileStorageService fileStorageService,
            IConflictCheckService conflictCheckService,
            ICacheService cacheService)
        {
            _unitOfWork = unitOfWork;
            _fileStorageService = fileStorageService;
            _conflictCheckService = conflictCheckService;
            _cacheService = cacheService;
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

        public async Task<PagedResultDto<CampaignCatalogDto>> GetCampaignCatalogPagedAsync(int pageNumber, int pageSize)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 12;
            if (pageSize > 100) pageSize = 100; 

            var (campaigns, totalCount) = await _unitOfWork.Campaigns.GetCampaignCatalogPagedAsync(pageNumber, pageSize);

            var campaignIds = campaigns.Select(c => c.Id).ToList();
            var campaignIdsWithSlots = await _unitOfWork.Slots.GetCampaignIdsWithAvailableSlotsAsync(campaignIds);

            var items = campaigns.Select(c => MapToCampaignCatalogDto(c, campaignIdsWithSlots.Contains(c.Id))).ToList();

            return new PagedResultDto<CampaignCatalogDto>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        // Добавь в класс CampaignService

        public async Task<PagedResultDto<CampaignCatalogDto>> GetCampaignCatalogFilteredAsync(CampaignFilterDto filter)
        {
            if (filter.PageNumber < 1) filter.PageNumber = 1;
            if (filter.PageSize < 1) filter.PageSize = 12;
            if (filter.PageSize > 100) filter.PageSize = 100;

            var (campaigns, totalCount) = await _unitOfWork.Campaigns.GetCampaignCatalogFilteredAsync(
                filter.Search,
                filter.Tag,
                filter.HasSlots,
                filter.SortBy,
                filter.PageNumber,
                filter.PageSize
            );

            var campaignIds = campaigns.Select(c => c.Id).ToList();
            var campaignIdsWithSlots = await _unitOfWork.Slots.GetCampaignIdsWithAvailableSlotsAsync(campaignIds);

            var items = campaigns
                .Select(c => MapToCampaignCatalogDto(c, campaignIdsWithSlots.Contains(c.Id)))
                .ToList();

            // Применяем фильтр по hasSlots после получения информации о слотах
            if (filter.HasSlots.HasValue)
            {
                items = items.Where(c => c.HasAvailableSlots == filter.HasSlots.Value).ToList();
                totalCount = items.Count; // Обновляем общее количество
            }

            return new PagedResultDto<CampaignCatalogDto>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize
            };
        }

        // --- Создание кампании ---
        public async Task<CampaignDto> CreateAsync(CreateCampaignDto dto, Guid? currentUserId = null, string role = "Master")
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                // Добавьте валидацию, если SupportedRoomTypes обязателен
                if (dto.SupportedRoomTypes == null || !dto.SupportedRoomTypes.Any())
                {
                    throw new ArgumentException("SupportedRoomTypes must contain at least one room type.");
                }

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

                // Получение комнат по типам (как в UpdateAsync)
                var roomIds = dto.SupportedRoomTypes.Any()
                    ? (await _unitOfWork.Campaigns.GetRoomsByTypesAsync(dto.SupportedRoomTypes.ToList()))
                          .Select(r => r.Id).ToList()
                    : new List<Guid>();

                var rooms = await _unitOfWork.Campaigns.GetRoomsByIdsAsync(roomIds);
                if (rooms.Count != roomIds.Count)
                {
                    var foundIds = rooms.Select(r => r.Id).ToHashSet();
                    var notFound = roomIds.Where(id => !foundIds.Contains(id)).ToList();
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

                if (dto.Tags != null && dto.Tags.Any())
                {
                    var uniqueTags = dto.Tags
                        .Select(tag => tag.Trim())
                        .Where(tag => !string.IsNullOrWhiteSpace(tag))
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .ToList();

                    campaign.Tags.AddRange(uniqueTags.Select(tag => new CampaignTag(tag, campaign.Id)));
                }

                await _unitOfWork.Campaigns.AddAsync(campaign);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                await _cacheService.RemoveAsync("campaigns_catalog");

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
                    var uniqueTags = dto.Tags
                        .Select(tag => tag.Trim())
                        .Where(tag => !string.IsNullOrWhiteSpace(tag))
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .ToList();

                    await _unitOfWork.Campaigns.UpdateCampaignTagsAsync(campaign.Id, uniqueTags);
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
                        ? (await _unitOfWork.Campaigns.GetRoomsByTypesAsync(dto.SupportedRoomTypes.ToList())) 
                              .Select(r => r.Id).ToList()
                        : new List<Guid>();

                    await _unitOfWork.Campaigns.UpdateCampaignRoomsAsync(campaign.Id, roomIds);
                }

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                await _cacheService.RemoveAsync("campaigns_catalog");

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

            await _cacheService.RemoveAsync("campaigns_catalog");
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

            await _cacheService.RemoveAsync("campaigns_catalog");

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
            const string cacheKey = "campaigns_catalog";

            // Пытаемся получить из кеша
            var cached = await _cacheService.GetAsync<List<CampaignCatalogDto>>(cacheKey);
            if (cached != null)
                return cached;

            // Если в кеше нет - берем из БД
            var campaigns = await _unitOfWork.Campaigns.GetCampaignCatalogAsync();
            var campaignIds = campaigns.Select(c => c.Id).ToList();
            var campaignIdsWithSlots = await _unitOfWork.Slots.GetCampaignIdsWithAvailableSlotsAsync(campaignIds);

            var result = campaigns.Select(c => MapToCampaignCatalogDto(c, campaignIdsWithSlots.Contains(c.Id))).ToList();

            // Сохраняем в кеш на 5 минут
            await _cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(5));

            return result;
        }

        public async Task<List<UpcomingGameDto>> GetUpcomingGamesAsync()
        {
            const string cacheKey = "upcoming_games";

            var cached = await _cacheService.GetAsync<List<UpcomingGameDto>>(cacheKey);
            if (cached != null)
                return cached;

            var slots = await _unitOfWork.Slots.GetUpcomingSlotsAsync();
            var result = slots.Select(MapToUpcomingGameDto).ToList();

            // Upcoming games меняются при букинге - 3 минуты
            await _cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(3));

            return result;
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
                CampaignImageUrl = campaign.ImageUrl,
                Level = campaign.Level,
                StartTime = slot.StartTime,
                MaxPlayers = campaign.MaxPlayers,
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