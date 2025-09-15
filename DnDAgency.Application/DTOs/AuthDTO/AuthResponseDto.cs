using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DnDAgency.Application.DTOs.UsersDTO;

namespace DnDAgency.Application.DTOs.AuthDTO
{
    public class AuthResponseDto
    {
        public string Token { get; set; } = null!;
        public UserDto User { get; set; } = null!;
        public DateTime Expires { get; set; }
        public string RefreshToken { get; set; } = null!;
    }
}
