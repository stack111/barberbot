using System;
using System.Threading.Tasks;

namespace BarberBot
{
    public interface IHoursRepository<T>
    {
        Task<bool> IsAvailableAsync(T instance, DateTime dateTime);
    }
}