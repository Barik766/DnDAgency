using DnDAgency.Application.DTOs.MastersDTO;
using DnDAgency.Domain.Enums;

namespace DnDAgency.Application.DTOs.UsersDTO
{
    public class UserDto
    {
        public Guid Id { get; set; }
        public string Username { get; set; } = null!;
        public string Email { get; set; } = null!;
        public UserRole Role { get; set; }
        public bool IsMaster { get; set; }
        public bool IsAdmin { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public MasterDto? MasterProfile { get; set; }
    }
}