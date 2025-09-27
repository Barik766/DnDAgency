using DnDAgency.Application.DTOs.AuthDTO;
using DnDAgency.Application.DTOs.MastersDTO;
using DnDAgency.Application.DTOs.UsersDTO;
using DnDAgency.Application.Interfaces;
using DnDAgency.Domain.Entities;
using DnDAgency.Infrastructure.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace DnDAgency.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;

        public UserService(
            IUnitOfWork unitOfWork,
            IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _configuration = configuration;
        }

        public async Task<UserDto> CreateAsync(string username, string email, string password)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var existingUser = await _unitOfWork.Users.GetByEmailAsync(email);
                if (existingUser != null)
                    throw new ArgumentException("User with this email already exists");

                var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);
                var user = new User(username, email, hashedPassword);

                await _unitOfWork.Users.AddAsync(user);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();
                
                return MapToDto(user);
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<List<UserDto>> GetAllAsync()
        {
            var users = await _unitOfWork.Users.GetAllAsync();
            return users.Where(u => u.IsActive).Select(MapToDto).ToList();
        }

        public async Task<UserDto> GetByIdAsync(Guid id)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(id);
            if (user == null || !user.IsActive)
                throw new KeyNotFoundException("User not found");

            return MapToDto(user);
        }

        public async Task<UserDto> UpdateAsync(Guid id, UpdateUserDto dto, Guid currentUserId)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var user = await _unitOfWork.Users.GetByIdAsync(id);
                if (user == null)
                    throw new KeyNotFoundException("User not found");

                if (id != currentUserId)
                    throw new UnauthorizedAccessException("You can only update your own profile");

                if (!string.IsNullOrEmpty(dto.Username))
                    user.UpdateUsername(dto.Username);

                if (!string.IsNullOrEmpty(dto.Email))
                {
                    var existingUser = await _unitOfWork.Users.GetByEmailAsync(dto.Email);
                    if (existingUser != null && existingUser.Id != id)
                        throw new ArgumentException("Email is already in use");

                    user.UpdateEmail(dto.Email);
                }

                _unitOfWork.Users.Update(user);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();
                
                return MapToDto(user);
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<AuthResponseDto> AuthenticateAsync(string email, string password)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var user = await _unitOfWork.Users.GetByEmailAsync(email);
                if (user == null || !user.IsActive || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
                    throw new UnauthorizedAccessException("Invalid credentials");

                var accessToken = GenerateJwtToken(user);
                var refreshToken = GenerateRefreshToken(user.Id);

                await _unitOfWork.RefreshTokens.AddAsync(refreshToken);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                return new AuthResponseDto
                {
                    Token = accessToken,
                    RefreshToken = refreshToken.Token,
                    User = MapToDto(user),
                    Expires = DateTime.UtcNow.AddMinutes(int.Parse(_configuration["JwtSettings:ExpiryMinutes"]!))
                };
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<AuthResponseDto> RefreshTokenAsync(string token)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var refreshToken = await _unitOfWork.RefreshTokens.GetByTokenAsync(token);
                if (refreshToken == null || !refreshToken.IsActive)
                    throw new UnauthorizedAccessException("Invalid or inactive refresh token");

                // деактивируем старый
                refreshToken.Revoke();
                _unitOfWork.RefreshTokens.Update(refreshToken);

                var user = await _unitOfWork.Users.GetByIdAsync(refreshToken.UserId);
                if (user == null || !user.IsActive)
                    throw new UnauthorizedAccessException("User not found");

                var newAccessToken = GenerateJwtToken(user);
                var newRefreshToken = GenerateRefreshToken(user.Id);

                await _unitOfWork.RefreshTokens.AddAsync(newRefreshToken);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                return new AuthResponseDto
                {
                    Token = newAccessToken,
                    RefreshToken = newRefreshToken.Token,
                    User = MapToDto(user),
                    Expires = DateTime.UtcNow.AddMinutes(int.Parse(_configuration["JwtSettings:ExpiryMinutes"]!))
                };
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task ChangePasswordAsync(Guid userId, ChangePasswordDto dto)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null)
                throw new KeyNotFoundException("User not found");

            if (!BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, user.PasswordHash))
                throw new UnauthorizedAccessException("Current password is incorrect");

            var newHashedPassword = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            user.UpdatePassword(newHashedPassword);

            _unitOfWork.Users.Update(user);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task DeactivateAsync(Guid id, Guid currentUserId)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(id);
            if (user == null)
                throw new KeyNotFoundException("User not found");

            if (id != currentUserId)
                throw new UnauthorizedAccessException("You can only deactivate your own account");

            user.Deactivate();
            _unitOfWork.Users.Update(user);
            await _unitOfWork.SaveChangesAsync();
        }

        private string GenerateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["JwtSettings:Key"]!);
            var expiresMinutes = int.Parse(_configuration["JwtSettings:ExpiryMinutes"]!);

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Name, user.Username),
                new(ClaimTypes.Email, user.Email),
                new(ClaimTypes.Role, user.Role.ToString())
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(expiresMinutes),
                Issuer = _configuration["JwtSettings:Issuer"],
                Audience = _configuration["JwtSettings:Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private static RefreshToken GenerateRefreshToken(Guid userId)
        {
            var randomBytes = RandomNumberGenerator.GetBytes(64);
            var token = Convert.ToBase64String(randomBytes);

            return new RefreshToken(
                token: token,
                expires: DateTime.UtcNow.AddDays(7),
                userId: userId
            );
        }

        private static UserDto MapToDto(User user)
        {
            return new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role.ToString(),
                IsMaster = user.IsMaster,
                IsAdmin = user.IsAdmin,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt,
                MasterProfile = user.MasterProfile != null ? MapMasterToDto(user.MasterProfile) : null
            };
        }

        private static MasterDto MapMasterToDto(Master master)
        {
            return new MasterDto
            {
                Id = master.Id,
                UserId = master.UserId,
                Name = master.Name,
                Bio = master.Bio,
                IsActive = master.IsActive,
                CampaignCount = master.Campaigns.Count(c => c.IsActive),
                AverageRating = master.Reviews.Any() ? master.Reviews.Average(r => r.Rating) : 0,
                ReviewCount = master.Reviews.Count,
                CreatedAt = master.CreatedAt,
                UpdatedAt = master.UpdatedAt
            };
        }
    }
}