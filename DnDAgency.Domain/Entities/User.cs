using DnDAgency.Domain.Enums;
using System.Text.RegularExpressions;

namespace DnDAgency.Domain.Entities
{
    public class User
    {
        public Guid Id { get; private set; } = Guid.NewGuid();
        public string Username { get; private set; }
        public string Email { get; private set; }
        public string PasswordHash { get; private set; }
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; private set; }
        public bool IsActive { get; private set; } = true;
        public UserRole Role { get; private set; } = UserRole.Player;

        // Профиль мастера (если есть)
        public Master? MasterProfile { get; private set; }

        public List<Booking> Bookings { get; private set; } = new();

        public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
        public string? GoogleId { get; private set; }
        public bool IsGoogleUser => !string.IsNullOrEmpty(GoogleId);


        // Конструктор для EF Core
        protected User() { }

        // Конструктор для обычного пользователя с паролем
        public User(string username, string email, string passwordHash)
        {
            ValidateUsername(username);
            ValidateEmail(email);
            ValidatePasswordHash(passwordHash);

            Username = username;
            Email = email;
            PasswordHash = passwordHash;
        }

        // Конструктор для Google-пользователя
        public User(string username, string email, string googleId, bool isGoogleAuth)
        {
            if (!isGoogleAuth)
                throw new ArgumentException("Use regular constructor for non-Google users");

            ValidateUsername(username);
            ValidateEmail(email);

            Username = username;
            Email = email;
            GoogleId = googleId;
            PasswordHash = string.Empty; // Пароль не нужен
        }

        public void UpdateUsername(string username)
        {
            ValidateUsername(username);
            Username = username;
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdateEmail(string email)
        {
            ValidateEmail(email);
            Email = email;
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdatePassword(string passwordHash)
        {
            ValidatePasswordHash(passwordHash);
            PasswordHash = passwordHash;
            UpdatedAt = DateTime.UtcNow;
        }

        public void PromoteToMaster()
        {
            if (Role != UserRole.Player)
                throw new InvalidOperationException($"Cannot promote user with role {Role} to Master");
            Role = UserRole.Master;
            UpdatedAt = DateTime.UtcNow;
        }

        public void PromoteToAdmin()
        {
            Role = UserRole.Admin;
            UpdatedAt = DateTime.UtcNow;
        }

        public void DemoteToPlayer()
        {
            if (Role == UserRole.Admin)
                throw new InvalidOperationException("Cannot demote Admin to Player directly");
            Role = UserRole.Player;
            UpdatedAt = DateTime.UtcNow;
        }

        public Master CreateMasterProfile(string bio)
        {
            if (MasterProfile != null)
                throw new InvalidOperationException("Master profile already exists");

            if (Role != UserRole.Master && Role != UserRole.Admin)
                throw new InvalidOperationException("User must have Master or Admin role");

            MasterProfile = new Master(Id, Username, bio);
            UpdatedAt = DateTime.UtcNow;
            return MasterProfile;
        }

        public bool IsMaster => Role == UserRole.Master || Role == UserRole.Admin;
        public bool IsAdmin => Role == UserRole.Admin;

        public void Deactivate()
        {
            IsActive = false;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Activate()
        {
            IsActive = true;
            UpdatedAt = DateTime.UtcNow;
        }

        private static void ValidateUsername(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentException("Username cannot be empty");
            if (username.Length < 3 || username.Length > 50)
                throw new ArgumentException("Username must be between 3 and 50 characters");
            if (!Regex.IsMatch(username, @"^[\w-]+$"))
                throw new ArgumentException("Username can only contain letters, numbers, underscores and hyphens");
        }

        private static void ValidateEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email cannot be empty");
            if (!System.Text.RegularExpressions.Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                throw new ArgumentException("Invalid email format");
            if (email.Length > 100)
                throw new ArgumentException("Email cannot exceed 100 characters");
        }

        private static void ValidatePasswordHash(string passwordHash)
        {
            if (string.IsNullOrWhiteSpace(passwordHash))
                throw new ArgumentException("Password hash cannot be empty");
        }
    }
}