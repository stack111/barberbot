using System;
using System.Linq;
using System.Threading.Tasks;

namespace BarberBot
{
    [Serializable]
    public class Appointment
    {
        private readonly IRepository<Appointment> repository;

        public IShop Shop { get; private set; }
        public IBarber Barber { get; private set; }
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

        public async Task<bool> HasConflictingAppointmentsAsync()
        {
            var appointments = await repository.LoadAllByDateTimeAsync(this);
            return appointments.Any();
        }

        public bool IsConflicting(Appointment appointment)
        {
            if (!Barber.Equals(appointment.Barber))
            {
                return false;
            }

            if (StartDateTime == appointment.StartDateTime)
            {
                return true;
            }
            else if (StartDateTime < appointment.StartDateTime)
            {
                if (EndDateTime <= appointment.StartDateTime)
                {
                    return false;
                }
                else if (StartDateTime > appointment.EndDateTime)
                {
                    return true;
                }
                else if (EndDateTime < appointment.EndDateTime ||
                    EndDateTime >= appointment.EndDateTime)
                {
                    return true;
                }
            }
            else // StartDateTime > appointment.StartDateTime
            {
                if (StartDateTime >= appointment.EndDateTime)
                {
                    return false; // adjacent
                }
                else if (EndDateTime != appointment.EndDateTime)
                {
                    return true;
                }
            }

            return false;
        }

        public override bool Equals(object obj)
        {
            Appointment other = obj as Appointment;
            if(other == null)
            {
                return false;
            }

            return Shop == other.Shop && Barber == other.Barber && StartDateTime == other.StartDateTime && EndDateTime == other.EndDateTime;
        }

        public override int GetHashCode()
        {
            return Shop.GetHashCode() ^ Barber.GetHashCode() ^ StartDateTime.GetHashCode() ^ EndDateTime.GetHashCode();
        }

        public override string ToString()
        {
            return $"{Barber.DisplayName} at {StartDateTime} to {EndDateTime}";
        }
    }
}