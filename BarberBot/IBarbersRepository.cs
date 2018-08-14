using System.Threading.Tasks;

namespace BarberBot
{
    public interface IBarbersRepository
    {
        Task LoadAsync(Shop shop, bool withAnyone);
    }
}