using System;
using System.Threading.Tasks;

namespace BarberBot
{
    public interface IRepository<T>
    {
        Task LoadHoursAsync(Barber instance, DateTime dateTime);
        Task<bool> IsAppointmentAvailableAsync(Barber instance, DateTime dateTime);
    }
}