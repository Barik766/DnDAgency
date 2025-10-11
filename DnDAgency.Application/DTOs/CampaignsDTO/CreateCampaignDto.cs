using Microsoft.AspNetCore.Http;

public class CreateCampaignDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public IFormFile? ImageFile { get; set; }
    public int Level { get; set; }
    public string[] SupportedRoomTypes { get; set; } = Array.Empty<string>();  // Заменил List<string> на string[] — теперь биндинг сработает
    public int MaxPlayers { get; set; } = 8;
    public List<string> Tags { get; set; } = new();
    public int? DurationHours { get; set; }
    public List<Guid> MasterIds { get; set; } = new();
}