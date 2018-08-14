using System;
using System.Threading.Tasks;

namespace BarberBot
{
    [Serializable]
    public abstract class Hours<T> where T : ISchedulable
    {
        public const int UTC_to_PST_Hours = -7;
        public int OpeningHour { get; set; }
        public int ClosingHour { get; set; }
        protected DateTime Requested { get; set; }
        protected readonly IHoursRepository Repository;
        public T Instance { get; private set; }

        public Hours(Hours<T> hours) : this(hours.Repository)
        {
            OpeningHour = hours.OpeningHour;
            ClosingHour = hours.ClosingHour;
            Requested = hours.Requested;
            Instance = hours.Instance;
        }

        public Hours(IHoursRepository Repository)
        {
            this.Repository = Repository;
        }

        public bool Exists
        {
            get
            {
                return !(OpeningHour == 0 && ClosingHour == 0);
            }
        }

        public async Task<bool> IsAvailableAsync(DateTime dateTime)
        {
            await LoadAsync(Instance, dateTime);
            return Exists && IsWithinHours(dateTime);
        }

        public async Task LoadAsync(T instance, DateTime dateTime)
        {
            Instance = instance;
            Requested = dateTime;
            await Repository.LoadAsync(this, dateTime);
        }

        public string FormattedDayHours()
        {
            if (Exists)
            {
                DateTime opening = OpeningDateTime();
                return $"{opening.Date.ToString("ddd")} {String.Format("{0:t}", opening)} - {String.Format("{0:t}", ClosingDateTime())}";
            }
            else
            {
                return NotAvailableHoursString;
            }
        }

        public DateTime ClosingDateTime()
        {
            return new DateTime(Requested.Year, Requested.Month, Requested.Day, ClosingHour, 0, 0, DateTimeKind.Local);
        }

        public DateTime OpeningDateTime()
        {
            return new DateTime(Requested.Year, Requested.Month, Requested.Day, OpeningHour, 0, 0, DateTimeKind.Local);
        }

        public bool IsWithinHours(DateTime dateTime)
        {
            var opening = OpeningDateTime();
            var closing = ClosingDateTime();
            return dateTime >= opening && dateTime <= closing;
        }

        public static TimeSpan AppointmentLength = TimeSpan.FromHours(1);
        public static TimeSpan AppointmentMiddleLength = TimeSpan.FromMinutes(30);

        protected abstract string NotAvailableHoursString { get; }
    }
}