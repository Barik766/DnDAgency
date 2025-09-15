using Microsoft.AspNetCore.Http;

public class UpdateCampaignDto
{
    public string Title { get; set; }
    public string Description { get; set; }
    public decimal? Price { get; set; }
    public IFormFile? ImageFile { get; set; }
    public int? Level { get; set; }
    public int? MaxPlayers { get; set; }
    public List<string> Tags { get; set; }
    public int? DurationHours { get; set; } 
}
