using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BarberBot
{
    [Serializable]
    public class MemoryBarberRepository : IHoursRepository<Barber>
    {
        private readonly Dictionary<string, BarberHours> BarberHours;
        private readonly IRepository<Appointment> appointmentRepository;
        public MemoryBarberRepository(IRepository<Appointment> appointmentRepository)
        {
            BarberHours = new Dictionary<string, BarberHours>()
            {
                { "Jessica", new BarberHours()
                            {
                            }
                }
            };
            this.appointmentRepository = appointmentRepository;
        }

        public async Task<bool> IsAvailableAsync(Barber instance, DateTime dateTime)
        {
            return !await appointmentRepository.ExistsAsync(instance.DisplayName, dateTime);
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