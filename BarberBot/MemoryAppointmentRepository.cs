using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BarberBot
{
    [Serializable]
    public class MemoryAppointmentRepository : IRepository<Appointment>
    {
        private static Dictionary<string, List<Appointment>> BookedAppointments = new Dictionary<string, List<Appointment>>();

        public Task<IEnumerable<Appointment>> LoadAllByDateTimeAsync(Appointment instance)
        {
            if (BookedAppointments.ContainsKey(instance.Barber.DisplayName))
            {
                var appointments = BookedAppointments[instance.Barber.DisplayName];
                return Task.FromResult(appointments.Where(a => a.IsConflicting(instance)));
            }
            else
            {
                IEnumerable<Appointment> empty = new List<Appointment>();
                return Task.FromResult(empty);
            }
        }

        public Task SaveAsync(Appointment instance)
        {
            if (BookedAppointments.ContainsKey(instance.Barber.DisplayName))
            {
                List<Appointment> appointments = BookedAppointments[instance.Barber.DisplayName];
                appointments = appointments ?? new List<Appointment>();
                List<Appointment> updatedAppointments = new List<Appointment>(appointments.Capacity);
                var existingAppointment = appointments.FirstOrDefault(a => a.Equals(instance));
                if(existingAppointment != null)
                {
                    var i = appointments.IndexOf(existingAppointment);
                    appointments[i] = instance;
                }
                else
                {
                    updatedAppointments.Add(instance);
                }
               
                BookedAppointments[instance.Barber.DisplayName] = updatedAppointments;
            }
            else
            {
                List<Appointment> updatedAppointments = new List<Appointment>
                {
                    instance
                };
                BookedAppointments.Add(instance.Barber.DisplayName, updatedAppointments);
            }
            return Task.CompletedTask;
        }
    }
}