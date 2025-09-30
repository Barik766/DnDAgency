using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DnDAgency.Infrastructure.Interfaces
{
    public interface IGoogleOAuthService
    {
        Task<GoogleUserInfo> ValidateGoogleTokenAsync(string idToken);
        Task<GoogleUserInfo> ExchangeCodeForUserInfoAsync(string code, string redirectUri);
    }

    public class GoogleUserInfo
    {
        public string Email { get; set; }
        public string Name { get; set; }
        public string GoogleId { get; set; }
        public string? Picture { get; set; }
    }
}
