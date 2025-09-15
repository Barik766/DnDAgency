using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DnDAgency.Application.DTOs.AuthDTO
{
    public class RefreshTokenDto
    {
        public string Token { get; set; } = null!;
        public DateTime Expires { get; set; }
    }
}
