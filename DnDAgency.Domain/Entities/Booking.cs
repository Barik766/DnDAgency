namespace DnDAgency.Domain.Entities
{
    public class Booking
    {
        public Guid Id { get; private set; } = Guid.NewGuid();
        public Guid UserId { get; private set; }
        public User User { get; private set; }
        public Guid SlotId { get; private set; }
        public Slot Slot { get; private set; }
        public int PlayersCount { get; private set; }
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

        private Booking() { } // EF Core

        public Booking(Guid userId, Guid slotId, int playersCount = 1)
        {
            ValidatePlayersCount(playersCount);

            UserId = userId;
            SlotId = slotId;
            PlayersCount = playersCount;
        }

        public void UpdatePlayersCount(int playersCount)
        {
            ValidatePlayersCount(playersCount);
            PlayersCount = playersCount;
        }

        private static void ValidatePlayersCount(int playersCount)
        {
            if (playersCount < 1)
                throw new ArgumentException("Players count must be at least 1");
            if (playersCount > 8)
                throw new ArgumentException("Players count cannot exceed 8");
        }
    }
}