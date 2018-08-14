using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace BarberBot
{
    [Serializable]
    public class MemoryHoursRepository : IHoursRepository
    {
        private readonly Dictionary<string, BarberHours> BarberHours;

        public MemoryHoursRepository()
        {
            BarberHours = new Dictionary<string, BarberHours>()
            {
                { "Jessica", new BarberHours(this)
                            {
                            }
                }
            };
        }

        public async Task<bool> IsAvailableAsync(Barber instance, DateTime dateTime)
        {
            await LoadAsync(instance.Hours, dateTime);
            return instance.Hours.Exists && instance.Hours.IsWithinHours(dateTime);
        }

        public Task LoadAsync<T>(Hours<T> hours, DateTime dateTime) where T : ISchedulable
        {
            switch (hours.Instance.Type)
            {
                case HoursType.Barber:
                    LoadBarberHours(hours, dateTime);
                    break;
                case HoursType.Shop:
                    LoadShopHours(hours, dateTime);
                    break;
            }
            return Task.CompletedTask;
        }

        void LoadShopHours<T>(Hours<T> instance, DateTime dateTime) where T : ISchedulable
        {
            // todo: special case check for holidays

            int openHour = 0, closeHour = 0;
            switch (dateTime.DayOfWeek)
            {
                case DayOfWeek.Sunday:
                    openHour = 8;
                    closeHour = 18;
                    break;
                case DayOfWeek.Monday:
                case DayOfWeek.Tuesday:
                case DayOfWeek.Wednesday:
                case DayOfWeek.Thursday:
                case DayOfWeek.Friday:
                case DayOfWeek.Saturday:
                    openHour = 8;
                    closeHour = 20;
                    break;
            }

            instance.OpeningHour = openHour;
            instance.ClosingHour = closeHour;
        }
        
        void LoadBarberHours<T>(Hours<T> instance, DateTime dateTime) where T : ISchedulable
        {
            // todo: special case check for holidays
            
            int openHour = 0, closeHour = 0;
            switch (dateTime.DayOfWeek)
            {
                case DayOfWeek.Sunday:
                    openHour = 0;
                    closeHour = 0;
                    break;
                case DayOfWeek.Monday:
                case DayOfWeek.Tuesday:
                    openHour = 8;
                    closeHour = 20;
                    break;
                case DayOfWeek.Wednesday:
                    openHour = 0;
                    closeHour = 0;
                    break;
                case DayOfWeek.Thursday:
                case DayOfWeek.Friday:
                case DayOfWeek.Saturday:
                    openHour = 8;
                    closeHour = 20;
                    break;
            }

            instance.OpeningHour = openHour;
            instance.ClosingHour = closeHour;
        }
    }
}