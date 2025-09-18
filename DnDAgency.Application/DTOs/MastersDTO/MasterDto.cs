namespace DnDAgency.Application.DTOs.MastersDTO
{
    public class MasterDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Name { get; set; } = null!;
        public string Bio { get; set; } = null!;
        public bool IsActive { get; set; }
        public int CampaignCount { get; set; }
        public double AverageRating { get; set; }
        public int ReviewCount { get; set; }
        public string? PhotoUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}