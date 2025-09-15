using System.ComponentModel.DataAnnotations;

namespace DnDAgency.Application.DTOs.MastersDTO
{
    public class CreateMasterProfileDto
    {
        [Required]
        [StringLength(2000, MinimumLength = 10)]
        public string Bio { get; set; } = null!;
    }
}