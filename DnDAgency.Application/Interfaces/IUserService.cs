using DnDAgency.Application.DTOs.AuthDTO;
using DnDAgency.Application.DTOs.UsersDTO;

namespace DnDAgency.Application.Interfaces
{
    public interface IUserService
    {
        Task<UserDto> GetByIdAsync(Guid id);
        Task<List<UserDto>> GetAllAsync();
        Task<UserDto> CreateAsync(string username, string email, string password);
        Task<UserDto> UpdateAsync(Guid id, UpdateUserDto dto, Guid currentUserId);
        Task<AuthResponseDto> AuthenticateAsync(string email, string password);
        Task ChangePasswordAsync(Guid userId, ChangePasswordDto dto);
        Task DeactivateAsync(Guid id, Guid currentUserId);
        Task<AuthResponseDto> RefreshTokenAsync(string refreshToken);
        Task<AuthResponseDto> AuthenticateWithGoogleAsync(string googleIdToken);
        Task<AuthResponseDto> AuthenticateWithGoogleCodeAsync(string code, string redirectUri);

    }
}