using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace BarberBot
{
    [Serializable]
    public class MemoryBarbersRepository : IBarbersRepository
    {
        private readonly IRepository<Appointment> appointmentRepository;
        private readonly IHoursRepository hoursRepository;

        public MemoryBarbersRepository(IRepository<Appointment> appointmentRepository, IHoursRepository hoursRepository)
        {
            this.appointmentRepository = appointmentRepository;
            this.hoursRepository = hoursRepository;
        }

        public async Task LoadAsync(Shop shop, bool withAnyone)
        {
            List<Barber> barbers = new List<Barber>()
            {
                new Barber(shop, appointmentRepository, new BarberHours(hoursRepository)) { DisplayName = "Jessica" }
            };
            if (withAnyone)
            {
                barbers.Add(new Barber(shop, appointmentRepository, new BarberHours(hoursRepository)) { DisplayName = "Anyone" });
            }

            shop.Barbers.Clear();
            shop.Barbers.AddRange(barbers);
        }
    }
}