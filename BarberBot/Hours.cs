using System;

namespace BarberBot
{
    [Serializable]
    public abstract class Hours<T>
    {
        public const int UTC_to_PST_Hours = -7;
        public int OpeningHour { get; protected set; }
        public int ClosingHour { get; protected set; }
        public abstract DateTime DateTime { get; protected set; }

        public bool Exists
        {
            get
            {
                return !(OpeningHour == 0 && ClosingHour == 0);
            }
        }

        public abstract void Load(T instance, DateTime dateTime);

        public abstract string FormattedDayHours();

        public DateTime ClosingDateTime()
        {
            return new DateTime(DateTime.Year, DateTime.Month, DateTime.Day, ClosingHour, 0, 0, DateTimeKind.Local);
        }

        public DateTime OpeningDateTime()
        {
            return new DateTime(DateTime.Year, DateTime.Month, DateTime.Day, OpeningHour, 0, 0, DateTimeKind.Local);
        }

        public bool IsWithinHours(DateTime dateTime)
        {
            var opening = OpeningDateTime();
            var closing = ClosingDateTime();
            return dateTime >= opening && dateTime <= closing;
        }

        public static TimeSpan AppointmentLength = TimeSpan.FromHours(1);
        public static TimeSpan AppointmentMiddleLength = TimeSpan.FromMinutes(30);
    }
}