using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BarberBot
{
    public interface IRepository<T>
    {
        Task<IEnumerable<T>> LoadAllByDateTimeAsync(T instance);
        Task SaveAsync(T instance);
    }
}