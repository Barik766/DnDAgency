using DnDAgency.Domain.Exceptions;

namespace DnDAgency.Domain.Entities
{
    public class Slot
    {
        public Guid Id { get; private set; } = Guid.NewGuid();
        public Guid CampaignId { get; private set; }
        public Campaign Campaign { get; private set; }
        public DateTime StartTime { get; private set; }

        public List<Booking> Bookings { get; private set; } = new();

        public int CurrentPlayers => Bookings.Count;
        public int AvailableSlots => Campaign.MaxPlayers - CurrentPlayers;
        public bool IsFull => AvailableSlots <= 0;
        public bool IsInPast => StartTime < DateTime.UtcNow;

        private Slot() { } // EF Core

        public Slot(Guid campaignId, DateTime startTime)
        {
            if (startTime < DateTime.UtcNow)
                throw new PastSlotBookingException();

            CampaignId = campaignId;
            StartTime = startTime;
        }

        public void UpdateStartTime(DateTime startTime)
        {
            ValidateStartTime(startTime, Campaign);
            StartTime = startTime;
        }

        public bool CanBeBooked(int maxPlayers)
        {
            return !IsInPast && CurrentPlayers < maxPlayers;
        }

        private static void ValidateStartTime(DateTime startTime, Campaign campaign)
        {
            if (startTime < DateTime.UtcNow)
                throw new PastSlotBookingException();

            var timeOfDay = startTime.TimeOfDay;
            
            if (timeOfDay < campaign.WorkingHoursStart)
                throw new ArgumentException($"Start time cannot be earlier than {campaign.WorkingHoursStart}");
            
            if (timeOfDay > campaign.GetMaxStartTime())
                throw new ArgumentException($"Start time cannot be later than {campaign.GetMaxStartTime()}");
        }
    }
}