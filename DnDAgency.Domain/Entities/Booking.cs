using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DnDAgency.Domain.Entities
{
    public class Booking
    {
        public Guid Id { get; private set; } = Guid.NewGuid();
        public Guid UserId { get; private set; }
        public User User { get; private set; }

        public Guid SlotId { get; private set; }
        public Slot Slot { get; private set; }

        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

        public Booking(Guid userId, Guid slotId)
        {
            UserId = userId;
            SlotId = slotId;
        }
    }
}
