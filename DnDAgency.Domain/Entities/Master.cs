namespace DnDAgency.Domain.Entities
{
    public class Master
    {
        public Guid Id { get; private set; } = Guid.NewGuid();
        public Guid UserId { get; private set; }
        public User User { get; private set; }
        public string Name { get; private set; }
        public string Bio { get; private set; }
        public bool IsActive { get; private set; } = true;
        public string PhotoUrl { get; private set; }
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; private set; }

        public List<Campaign> Campaigns { get; private set; } = new();
        public List<Review> Reviews { get; private set; } = new();

        // Для Entity Framework
        private Master() { }

        // ИСПРАВЛЕННЫЙ конструктор
        public Master(Guid userId, string name, string bio)
        {
            ValidateName(name);
            ValidateBio(bio);

            UserId = userId;
            Name = name;
            Bio = bio;
            PhotoUrl = string.Empty;
        }

        public void UpdateBio(string bio)
        {
            ValidateBio(bio);
            Bio = bio;
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdateName(string name)
        {
            ValidateName(name);
            Name = name;
            UpdatedAt = DateTime.UtcNow;
        }

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

        private static void ValidateName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Name cannot be empty");
            if (name.Length > 100)
                throw new ArgumentException("Name cannot exceed 100 characters");
        }

        private static void ValidateBio(string bio)
        {
            if (string.IsNullOrWhiteSpace(bio))
                throw new ArgumentException("Bio cannot be empty");
            if (bio.Length > 2000)
                throw new ArgumentException("Bio cannot exceed 2000 characters");
        }
    }
}