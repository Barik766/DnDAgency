using System;
using System.Collections.Generic;
using DnDAgency.Application.DTOs.BookingsDTO;

namespace DnDAgency.Application.DTOs.CampaignsDTO
{
    public class CampaignDetailsDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string MasterName { get; set; } = null!;
        public decimal Price { get; set; }
        public string ImageUrl { get; set; } = null!;
        public int Level { get; set; }
        public int MaxPlayers { get; set; }
        public double? DurationHours { get; set; }
        public List<string> Tags { get; set; } = new();
        public bool IsActive { get; set; }

        // список слотов для выбора
        public List<SlotDto> Slots { get; set; } = new();
    }
}
