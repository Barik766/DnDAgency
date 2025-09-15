using System.ComponentModel.DataAnnotations;

namespace DnDAgency.Application.DTOs.UsersDTO
{
    public class UpdateUserDto
    {
        [StringLength(50, MinimumLength = 3)]
        [RegularExpression(@"^[a-zA-Z0-9_-]+$", ErrorMessage = "Username can only contain letters, numbers, underscores and hyphens")]
        public string? Username { get; set; }

        [EmailAddress]
        [StringLength(100)]
        public string? Email { get; set; }
    }
}