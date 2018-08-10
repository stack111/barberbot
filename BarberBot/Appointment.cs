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
        public DateTime StartDateTime { get; private set; }

        public Appointment(IRepository<Appointment> repository)
        {
            this.repository = repository;
        }

        public void CopyFrom(AppointmentRequest request)
        {
            Shop = request.Shop;
            Barber = request.RequestedBarber;
            StartDateTime = request.StartDateTime;
        }

        public async Task BookAsync()
        {
            await repository.SaveAsync(this);
        }

        public async Task<bool> ExistsAsync()
        {
            return await repository.ExistsAsync(this);
        }
    }
}