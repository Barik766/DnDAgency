using DnDAgency.Domain.Entities;
using DnDAgency.Domain.Interfaces;

namespace DnDAgency.Application.Services
{
    public class ConflictCheckService : IConflictCheckService
    {
        private readonly ICampaignRepository _campaignRepository;
        private readonly ISlotRepository _slotRepository;
        private readonly IRoomRepository _roomRepository;

        public ConflictCheckService(
            ICampaignRepository campaignRepository,
            ISlotRepository slotRepository,
            IRoomRepository roomRepository)
        {
            _campaignRepository = campaignRepository;
            _slotRepository = slotRepository;
            _roomRepository = roomRepository;
        }

        public async Task<bool> HasConflictAsync(Guid campaignId, DateTime startTime, TimeSpan duration)
        {
            var campaign = await _campaignRepository.GetByIdAsync(campaignId);
            if (campaign == null)
                throw new KeyNotFoundException("Campaign not found");

            var endTime = startTime.Add(duration);

            // Проверяем конфликты для каждого типа комнат в кампании
            foreach (var room in campaign.Rooms)
            {
                bool hasConflict = room.Type switch
                {
                    RoomType.Physical => await CheckPhysicalRoomConflictAsync(room.Id, startTime, endTime),
                    RoomType.Online => await CheckOnlineMasterConflictAsync(campaign.Masters, startTime, endTime),
                    _ => throw new ArgumentException("Unknown room type")
                };

                if (hasConflict) return true;
            }

            return false;
        }

        private async Task<bool> CheckPhysicalRoomConflictAsync(Guid roomId, DateTime startTime, DateTime endTime)
        {
            // Получаем все кампании в этой физической комнате
            var roomCampaigns = await _campaignRepository.GetByRoomIdAsync(roomId);

            foreach (var campaign in roomCampaigns)
            {
                if (!campaign.DurationHours.HasValue) continue;

                var campaignDuration = TimeSpan.FromHours(campaign.DurationHours.Value);

                // Получаем все слоты этой кампании в указанную дату
                var existingSlots = await _slotRepository.GetByCampaignAndDateAsync(campaign.Id, startTime.Date);


                foreach (var slot in existingSlots)
                {
                    // Игнорируем пустые слоты
                    if (slot.CurrentPlayers == 0)
                        continue;

                    var slotEndTime = slot.StartTime.Add(campaignDuration);

                    Console.WriteLine($"Checking slot: {slot.StartTime} vs checking time: {startTime}");

                    // Проверяем пересечение временных интервалов
                    if (TimeIntervalsOverlap(startTime, endTime, slot.StartTime, slotEndTime))
                        return true;
                }
            }

            return false;
        }

        private async Task<bool> CheckOnlineMasterConflictAsync(List<Master> masters, DateTime startTime, DateTime endTime)
        {
            if (!masters.Any())
                return true; // Нет мастеров = конфликт

            // Проверяем, есть ли хотя бы один свободный мастер
            foreach (var master in masters)
            {
                var isMasterFree = await IsMasterFreeAsync(master.Id, startTime, endTime);
                if (isMasterFree)
                    return false; // Есть свободный мастер = нет конфликта
            }

            return true; // Все мастера заняты = конфликт
        }

        private async Task<bool> IsMasterFreeAsync(Guid masterId, DateTime startTime, DateTime endTime)
        {
            // Получаем все онлайн кампании этого мастера
            var masterCampaigns = await _campaignRepository.GetOnlineCampaignsByMasterIdAsync(masterId);

            foreach (var campaign in masterCampaigns)
            {
                if (!campaign.DurationHours.HasValue) continue;

                var campaignDuration = TimeSpan.FromHours(campaign.DurationHours.Value);

                // Получаем слоты в указанную дату
                var existingSlots = await _slotRepository.GetByCampaignAndDateAsync(campaign.Id, startTime.Date);

                foreach (var slot in existingSlots)
                {
                    var slotEndTime = slot.StartTime.Add(campaignDuration);

                    // Если есть пересечение - мастер занят
                    if (TimeIntervalsOverlap(startTime, endTime, slot.StartTime, slotEndTime))
                        return false;
                }
            }

            return true; // Мастер свободен
        }

        public static bool TimeIntervalsOverlap(DateTime start1, DateTime end1, DateTime start2, DateTime end2)
        {
            return start1 < end2 && start2 < end1;
        }
    }
}