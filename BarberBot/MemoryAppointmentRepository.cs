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
            // look for appointments between the range of the appointmentTime - half appointment length to appointmentTime + length for the id
            // this works because we base on rounding rules of 1-15 => 0 - 16-29 => 30 etc..
            var existingKeyValuePairs = BookedAppointments.Where(appt => appt.Key >= appointmentTime.Add(BarberHours.AppointmentMiddleLength.Negate()) && appt.Key < appointmentTime.Add(BarberHours.AppointmentLength) && appt.Value != null && appt.Value.Any(b => string.Equals(b.Barber.DisplayName, id, StringComparison.OrdinalIgnoreCase)));
            return Task.FromResult(existingKeyValuePairs != null && existingKeyValuePairs.Any());
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