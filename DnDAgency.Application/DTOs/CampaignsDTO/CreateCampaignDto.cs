using Microsoft.AspNetCore.Http;

public class CreateCampaignDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public IFormFile? ImageFile { get; set; }
    public int Level { get; set; }
    public List<string> SupportedRoomTypes { get; set; } = new();  // Заменено с RoomIds на SupportedRoomTypes
    public int MaxPlayers { get; set; } = 8;
    public List<string> Tags { get; set; } = new();
    public int? DurationHours { get; set; }

    // Список мастеров для кампании. Админ может не указывать, мастер добавляется автоматически
    public List<Guid> MasterIds { get; set; } = new();
}