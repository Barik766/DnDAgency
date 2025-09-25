namespace DnDAgency.Application.DTOs.BookingsDTO
{
    public class AvailableTimeSlot
    {
        public DateTime DateTime { get; set; }
        public bool IsAvailable { get; set; }
        public int CurrentPlayers { get; set; }
        public int MaxPlayers { get; set; }
        public RoomType RoomType { get; set; } 
        public int AvailableSlots => MaxPlayers - CurrentPlayers;
    }
}