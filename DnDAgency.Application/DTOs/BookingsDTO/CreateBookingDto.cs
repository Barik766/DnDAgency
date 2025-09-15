using System.ComponentModel.DataAnnotations;

namespace DnDAgency.Application.DTOs.BookingsDTO
{
    public class CreateBookingDto
    {
        [Required]
        public Guid SlotId { get; set; }
    }
}