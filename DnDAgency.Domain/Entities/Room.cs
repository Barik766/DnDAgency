namespace DnDAgency.Domain.Entities
{
    public class Room
    {
        public Guid Id { get; private set; } = Guid.NewGuid();
        public List<Campaign> Campaigns { get; private set; } = new();
        public string Name { get; private set; }
        public RoomType Type { get; private set; }
        public int? Capacity { get; private set; } // Null для Online
        public bool IsActive { get; private set; } = true;
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;



        private Room() { } // EF Core

        public Room(string name, RoomType type, int? capacity = null)
        {
            ValidateName(name);
            ValidateCapacity(type, capacity);

            Name = name;
            Type = type;
            Capacity = capacity;
        }

        public void UpdateName(string name)
        {
            ValidateName(name);
            Name = name;
        }

        public void UpdateCapacity(int? capacity)
        {
            ValidateCapacity(Type, capacity);
            Capacity = capacity;
        }

        public void Deactivate() => IsActive = false;
        public void Activate() => IsActive = true;

        private static void ValidateName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Room name cannot be empty");
            if (name.Length > 100)
                throw new ArgumentException("Room name cannot exceed 100 characters");
        }

        private static void ValidateCapacity(RoomType type, int? capacity)
        {
            if (type == RoomType.Physical && (!capacity.HasValue || capacity <= 0))
                throw new ArgumentException("Physical room must have positive capacity");

            if (type == RoomType.Online && capacity.HasValue)
                throw new ArgumentException("Online room should not have capacity limit");
        }
    }
}

public enum RoomType
{
    Physical = 0,
    Online = 1
}