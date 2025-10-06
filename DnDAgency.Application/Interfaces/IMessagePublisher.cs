using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DnDAgency.Application.Interfaces
{
    public interface IMessagePublisher
    {
        Task PublishBookingConfirmationAsync<T>(T message) where T : class;
    }
}
