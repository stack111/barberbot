using System.Collections.Generic;
using System.Threading.Tasks;

namespace BarberBot
{
    public interface IBarber : ISchedulable
    {
        string DisplayName { get; set; }
        BarberHours Hours { get; }

        bool Equals(object obj);
        int GetHashCode();
        Task<BarberAvailabilityResponse> IsAvailableAsync(AppointmentRequest appointmentRequest);
        Task<List<BarberService>> LoadServicesAsync();
        Task<AppointmentRequest> NextAvailableRequestAsync(AppointmentRequest appointmentRequest);
        string ToString();
    }
}