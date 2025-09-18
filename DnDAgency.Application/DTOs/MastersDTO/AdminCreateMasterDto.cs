using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace DnDAgency.Application.DTOs.MastersDTO
{
    public class AdminCreateMasterDto
    {
        // Данные пользователя
        [Required]
        [StringLength(50, MinimumLength = 3)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(100, MinimumLength = 6)]
        public string Password { get; set; } = string.Empty;

        // Данные профиля мастера
        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(2000, MinimumLength = 10)]
        public string Bio { get; set; } = string.Empty;

        public IFormFile? Photo { get; set; }
    }
}
