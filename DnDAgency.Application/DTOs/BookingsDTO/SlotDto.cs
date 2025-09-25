namespace DnDAgency.Application.DTOs.BookingsDTO
{
    public class SlotDto
    {
        public Guid Id { get; set; }
        public Guid CampaignId { get; set; }
        public DateTime StartTime { get; set; }

        public int CurrentPlayers { get; set; }
        public int AvailableSlots { get; set; }
        public bool IsFull { get; set; }
        public bool IsInPast { get; set; }
        public RoomType RoomType { get; set; }
    }
}
