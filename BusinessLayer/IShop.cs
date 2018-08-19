using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BarberBot
{
    public interface IShop : ISchedulable
    {
        List<Barber> Barbers { get; set; }

        Task<BarberAvailabilityResponse> AnyBarbersAvailableAsync(AppointmentRequest appointmentRequest);
        Task<bool> CanAcceptCustomersAsync(DateTime dateTime);
        Task<DateTime> ClosingDateTimeAsync(DateTime dateTime);
        Task<string> FormattedWeekHoursAsync(DateTime dateTime);
        Task<AppointmentAvailabilityResponse> IsAvailableAsync(AppointmentRequest appointmentRequest);
        Task<bool> IsOpenAsync(DateTime dateTime);
        Task<List<Barber>> LoadBarbersAsync(bool withAnyone);
        Task<AppointmentRequest> NextAvailableBarberAsync(AppointmentRequest appointmentRequest);
        Task<DateTime> OpeningDateTimeAsync(DateTime dateTime);
    }
}