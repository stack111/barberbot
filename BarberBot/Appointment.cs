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
        public BarberService Service { get; set; }

        public DateTime EndDateTime
        {
            get
            {
                return StartDateTime.Add(Service.Duration);
            }
        }
        public Appointment(IRepository<Appointment> repository)
        {
            this.repository = repository;
        }

        public void CopyFrom(AppointmentRequest request)
        {
            Shop = request.Shop;
            Barber = request.RequestedBarber;
            StartDateTime = request.StartDateTime;
            Service = request.Service;
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