namespace DnDAgency.Application.DTOs.CampaignsDTO;

public class CampaignFilterDto
{
    public string? Search { get; set; }
    public string? Tag { get; set; }
    public bool? HasSlots { get; set; }
    public string SortBy { get; set; } = "title"; // title, priceAsc, priceDesc, level
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 12;
}