namespace DnDAgency.Domain.Entities
{
    public class RefreshToken
    {
        public Guid Id { get; private set; } = Guid.NewGuid();
        public string Token { get; private set; }
        public DateTime Expires { get; private set; }
        public DateTime Created { get; private set; } = DateTime.UtcNow;
        public DateTime? Revoked { get; private set; }

        public bool IsExpired => DateTime.UtcNow >= Expires;
        public bool IsActive => Revoked == null && !IsExpired;

        public Guid UserId { get; private set; }
        public User User { get; private set; }

        private RefreshToken() { } // EF Core

        public RefreshToken(string token, DateTime expires, Guid userId)
        {
            Token = token;
            Expires = expires;
            UserId = userId;
        }

        public void Revoke()
        {
            Revoked = DateTime.UtcNow;
        }
    }
}
