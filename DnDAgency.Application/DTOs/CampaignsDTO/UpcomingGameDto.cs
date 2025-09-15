using System;

namespace DnDAgency.Application.DTOs.CampaignsDTO
{
    public class UpcomingGameDto
    {
        public Guid CampaignId { get; set; }
        public string CampaignTitle { get; set; } = null!;
        public string CampaignImageUrl { get; set; } = null!;
        public int Level { get; set; }

        // ближайший слот
        public Guid SlotId { get; set; }
        public DateTime StartTime { get; set; }
        public int MaxPlayers { get; set; }
        public int BookedPlayers { get; set; }
        public int AvailableSlots { get; set; }
        public bool IsFull { get; set; }
    }
}
