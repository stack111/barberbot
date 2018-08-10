using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BarberBot
{
    [Serializable]
    public class MemoryBarberHoursRepository : IHoursRepository<Barber>
    {
        private readonly Dictionary<string, BarberHours> BarberHours;
        
        public MemoryBarberHoursRepository(IRepository<Appointment> appointmentRepository)
        {
            BarberHours = new Dictionary<string, BarberHours>()
            {
                { "Jessica", new BarberHours()
                            {
                            }
                }
            };
        }

        public async Task<bool> IsAvailableAsync(Barber instance, DateTime dateTime)
        {
            await LoadHoursAsync(instance, dateTime);
            return instance.Hours.Exists && instance.Hours.IsWithinHours(dateTime);
        }

        private Task LoadHoursAsync(Barber instance, DateTime dateTime)
        {
            var hours = BarberHours[instance.DisplayName];
            hours.Load(instance, dateTime);
            instance.Hours = hours;
            return Task.CompletedTask;
        }
    }
}