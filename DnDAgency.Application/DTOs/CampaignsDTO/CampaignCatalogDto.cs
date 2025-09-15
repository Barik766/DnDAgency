using System;
using System.Collections.Generic;

namespace DnDAgency.Application.DTOs.CampaignsDTO
{
    public class CampaignCatalogDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = null!;
        public string ImageUrl { get; set; } = null!;
        public int Level { get; set; }
        public decimal Price { get; set; }
        public List<string> Tags { get; set; } = new();
        public bool HasAvailableSlots { get; set; }
        public bool IsActive { get; set; }
    }
}
