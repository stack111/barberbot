using System;
using System.Threading.Tasks;

namespace BarberBot
{
    public interface IHoursRepository<T>
    {
        Task LoadHoursAsync(T instance, DateTime dateTime);
        Task<bool> IsAvailableAsync(T instance, DateTime dateTime);
    }
}