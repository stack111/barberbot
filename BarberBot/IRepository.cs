using System;
using System.Threading.Tasks;

namespace BarberBot
{
    public interface IRepository<T>
    {
        Task<bool> ExistsAsync(string id, DateTime appointmentTime);
        Task<bool> ExistsAsync(T instance);
        Task SaveAsync(T instance);
    }
}