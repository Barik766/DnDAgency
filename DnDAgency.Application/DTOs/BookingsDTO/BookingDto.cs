using DnDAgency.Application.DTOs.UsersDTO;

namespace DnDAgency.Application.DTOs.BookingsDTO
{
    public class BookingDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public UserDto User { get; set; } = null!;
        public Guid SlotId { get; set; }
        public SlotDto Slot { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
    }
}