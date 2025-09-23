using System.ComponentModel.DataAnnotations;

namespace DnDAgency.Application.DTOs.BookingsDTO
{
    public class CreateBookingDto
    {
        [Required]
        public Guid CampaignId { get; set; }

        [Required]
        public DateTime StartTime { get; set; }
    }
}