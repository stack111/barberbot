using System;
using System.Threading.Tasks;

namespace BarberBot
{
    [Serializable]
    public class Appointment
    {
        private readonly IRepository<Appointment> repository;

        public Shop Shop { get; private set; }
        public Barber Barber { get; private set; }
        public DateTime AppointmentDateTime { get; private set; }

        public Appointment(IRepository<Appointment> repository)
        {
            this.repository = repository;
        }

        public void CopyFrom(AppointmentRequest request)
        {
            Shop = request.Shop;
            Barber = request.RequestedBarber;
            AppointmentDateTime = request.RequestedDateTime;
        }

        public async Task BookAsync()
        {
            await repository.SaveAsync(this);
        }
    }
}