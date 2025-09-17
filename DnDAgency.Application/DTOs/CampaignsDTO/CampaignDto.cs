namespace DnDAgency.Application.DTOs.CampaignsDTO
{
    public class CampaignDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        // Список мастеров, которые проводят кампанию
        public List<Guid> MasterIds { get; set; } = new();
        public List<string> MasterNames { get; set; } = new();

        public decimal Price { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public int Level { get; set; }   // 1–20
        public List<string> Tags { get; set; } = new();
        public TimeSpan? Duration { get; set; }
        public int MaxPlayers { get; set; } // 1–8
        public bool IsActive { get; set; }
        public bool HasAvailableSlots { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
