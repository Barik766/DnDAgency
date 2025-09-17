namespace DnDAgency.Domain.Entities
{
    public class Campaign
    {
        public Guid Id { get; private set; } = Guid.NewGuid();
        public string Title { get; private set; }
        public string Description { get; private set; }
        public List<Master> Masters { get; private set; } = new();

        public decimal Price { get; private set; }

        public string ImageUrl { get; private set; }
        public int Level { get; private set; } // 1–20
        public int MaxPlayers { get; private set; } // 1–8
        public int? DurationHours { get; private set; }

        public List<CampaignTag> Tags { get; private set; } = new();
        public List<Slot> Slots { get; private set; } = new();

        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; private set; }
        public bool IsActive { get; private set; } = true;

        public bool HasAvailableSlots => Slots.Any(s => s.CanBeBooked(MaxPlayers));

        private Campaign() { } // EF Core

        public Campaign(
        string title,
        string description,
        decimal price,
        string imageUrl,
        int level,
        int maxPlayers = 8,
        int? durationHours = null,
        List<Master>? masters = null)
            {
                ValidateTitle(title);
                ValidateDescription(description);
                ValidatePrice(price);
                ValidateImageUrl(imageUrl);
                ValidateLevel(level);
                ValidateMaxPlayers(maxPlayers);

                Title = title;
                Description = description;
                Price = price;
                ImageUrl = imageUrl;
                Level = level;
                MaxPlayers = maxPlayers;
                DurationHours = durationHours;

                Masters = masters ?? new List<Master>();
            }


        public void Update(
            string title,
            string description,
            decimal price,
            string imageUrl,
            int level,
            int maxPlayers,
            int? durationHours)
        {
            UpdateTitle(title);
            UpdateDescription(description);
            UpdatePrice(price);
            UpdateImageUrl(imageUrl);
            UpdateLevel(level);
            UpdateMaxPlayers(maxPlayers);
            UpdateDuration(durationHours);
        }

        public void UpdateTitle(string title)
        {
            ValidateTitle(title);
            Title = title;
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdateDescription(string description)
        {
            ValidateDescription(description);
            Description = description;
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdatePrice(decimal price)
        {
            ValidatePrice(price);
            Price = price;
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdateImageUrl(string imageUrl)
        {
            ValidateImageUrl(imageUrl);
            ImageUrl = imageUrl;
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdateLevel(int level)
        {
            ValidateLevel(level);
            Level = level;
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdateMaxPlayers(int maxPlayers)
        {
            ValidateMaxPlayers(maxPlayers);
            MaxPlayers = maxPlayers;
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdateDuration(int? durationHours)
        {
            if (durationHours.HasValue && durationHours <= 0)
                throw new ArgumentException("Duration must be positive hours or null");
            DurationHours = durationHours;
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

        public Slot AddSlot(DateTime startTime)
        {
            var slot = new Slot(Id, startTime);
            Slots.Add(slot);
            return slot;
        }

        // === Validators ===
        private static void ValidateTitle(string title)
        {
            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentException("Title cannot be empty");
            if (title.Length > 100)
                throw new ArgumentException("Title cannot exceed 100 characters");
        }

        private static void ValidateDescription(string description)
        {
            if (string.IsNullOrWhiteSpace(description))
                throw new ArgumentException("Description cannot be empty");
            if (description.Length > 1000)
                throw new ArgumentException("Description cannot exceed 1000 characters");
        }

        private static void ValidatePrice(decimal price)
        {
            if (price < 0)
                throw new ArgumentException("Price cannot be negative");
            if (price > 99999)
                throw new ArgumentException("Price cannot exceed 99999");
        }

        private static void ValidateImageUrl(string imageUrl)
        {
            if (string.IsNullOrWhiteSpace(imageUrl))
                throw new ArgumentException("ImageUrl cannot be empty");
            if (imageUrl.Length > 500)
                throw new ArgumentException("ImageUrl cannot exceed 500 characters");
        }

        private static void ValidateLevel(int level)
        {
            if (level < 1 || level > 20)
                throw new ArgumentException("Level must be between 1 and 20");
        }

        private static void ValidateMaxPlayers(int maxPlayers)
        {
            if (maxPlayers < 1 || maxPlayers > 8)
                throw new ArgumentException("MaxPlayers must be between 1 and 8");
        }
    }
}
