using System;
using System.Threading.Tasks;

namespace BarberBot
{
    public interface IHoursRepository
    {
        Task LoadAsync<T>(Hours<T> hours, DateTime dateTime) where T : ISchedulable;
    }
}