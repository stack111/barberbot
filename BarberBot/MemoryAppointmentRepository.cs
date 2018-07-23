using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BarberBot
{
    [Serializable]
    public class MemoryAppointmentRepository : IRepository<Appointment>
    {
        private static Dictionary<DateTime, List<Appointment>> BookedAppointments = new Dictionary<DateTime, List<Appointment>>();

        public Task<bool> ExistsAsync(string id, DateTime appointmentTime)
        {
            if (BookedAppointments.ContainsKey(appointmentTime))
            {
                List<Appointment> appointments = BookedAppointments[appointmentTime];
                bool existingAppointment = appointments
                    .Any(appointment => string.Equals(id, appointment.Barber.DisplayName, StringComparison.OrdinalIgnoreCase));
                return Task.FromResult(existingAppointment);
            }
            else
            {
                return Task.FromResult(false);
            }
        }

        public Task<bool> ExistsAsync(Appointment instance)
        {
            // todo filter by shop
            if (BookedAppointments.ContainsKey(instance.AppointmentDateTime))
            {
                List<Appointment> appointments = BookedAppointments[instance.AppointmentDateTime];
                return Task.FromResult(!appointments.Any(appointment => string.Equals(instance.Barber.DisplayName, appointment.Barber.DisplayName, StringComparison.OrdinalIgnoreCase)));
            }
            else
            {
                return Task.FromResult(false);
            }
        }

        public Task SaveAsync(Appointment instance)
        {
            if (BookedAppointments.ContainsKey(instance.AppointmentDateTime))
            {
                List<Appointment> appointments = BookedAppointments[instance.AppointmentDateTime];
                appointments = appointments ?? new List<Appointment>();
                List<Appointment> updatedAppointments = new List<Appointment>(appointments.Capacity);
                foreach(var existingAppointment in appointments)
                {
                    if(!string.Equals(existingAppointment.Barber.DisplayName, instance.Barber.DisplayName, StringComparison.OrdinalIgnoreCase))
                    {
                        updatedAppointments.Add(existingAppointment);
                    }
                }
                updatedAppointments.Add(instance);
                BookedAppointments[instance.AppointmentDateTime] = updatedAppointments;
            }
            else
            {
                List<Appointment> updatedAppointments = new List<Appointment>
                {
                    instance
                };
                BookedAppointments.Add(instance.AppointmentDateTime, updatedAppointments);
            }
            return Task.CompletedTask;
        }
    }
}