namespace DnDAgency.Application.Messages;

public class BookingConfirmationMessage
{
    public Guid BookingId { get; set; }
    public Guid UserId { get; set; }
    public string UserEmail { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string CampaignTitle { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public int PlayersCount { get; set; }
    public DateTime CreatedAt { get; set; }
}