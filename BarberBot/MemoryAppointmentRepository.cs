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

        public Task<bool> ExistsAsync(Appointment instance)
        {
            var existingKeyValuePairs = BookedAppointments.Where(appt => 
            appt.Key == instance.Barber.DisplayName && 
            appt.Value != null && 
            appt.Value.Any(a => a.IsConflicting(instance)));

            return Task.FromResult(existingKeyValuePairs != null && existingKeyValuePairs.Any());
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