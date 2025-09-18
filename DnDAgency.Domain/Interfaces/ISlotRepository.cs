using DnDAgency.Domain.Entities;

namespace DnDAgency.Domain.Interfaces;

public interface ISlotRepository : IRepository<Slot>
{
    Task<List<Slot>> GetByCampaignIdAsync(Guid campaignId);
    Task<List<Slot>> GetAvailableSlotsByCampaignIdAsync(Guid campaignId);
    Task<List<Slot>> GetUserSlotsAsync(Guid userId);
    Task<List<Slot>> GetUpcomingSlotsAsync();
    Task<List<Guid>> GetCampaignIdsWithAvailableSlotsAsync(List<Guid> campaignIds);
}