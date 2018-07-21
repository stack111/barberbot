using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace BarberBot
{
    [Serializable]
    public class MemoryBarberRepository : IRepository<Barber>
    {
        private readonly Dictionary<string, BarberHours> BarberHours;
        private readonly Dictionary<DateTime, List<string>> BookedAppointments;
        public MemoryBarberRepository()
        {
            BarberHours = new Dictionary<string, BarberHours>()
            {
                { "Jessica", new BarberHours()
                            {
                            }
                }
            };
            BookedAppointments = new Dictionary<DateTime, List<string>>();
        }

        public Task<bool> IsAppointmentAvailableAsync(Barber instance, DateTime dateTime)
        {
            
            if (BookedAppointments.ContainsKey(dateTime))
            {
                List<string> barbers = BookedAppointments[dateTime];
                return Task.FromResult(!barbers.Any(name => string.Equals(instance.DisplayName, name, StringComparison.OrdinalIgnoreCase)));
            }
            else
            {
                return Task.FromResult(true);
            }
        }

        public Task LoadHoursAsync(Barber instance, DateTime dateTime)
        {
            var hours = BarberHours[instance.DisplayName];
            hours.Load(instance, dateTime);
            instance.Hours = hours;
            return Task.CompletedTask;
        }
    }
}