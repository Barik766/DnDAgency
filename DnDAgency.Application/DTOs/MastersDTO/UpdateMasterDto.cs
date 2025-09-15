using System.ComponentModel.DataAnnotations;

namespace DnDAgency.Application.DTOs.MastersDTO
{
    public class UpdateMasterDto
    {
        [StringLength(2000, MinimumLength = 10)]
        public string? Bio { get; set; }

        [StringLength(100, MinimumLength = 2)]
        public string? Name { get; set; }
    }
}