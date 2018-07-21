using System;

namespace BarberBot
{
    [Serializable]
    public abstract class Hours<T>
    {
        public int OpeningHour { get; protected set; }
        public int ClosingHour { get; protected set; }

        public bool Exists
        {
            get
            {
                return !(OpeningHour == 0 && ClosingHour == 0);
            }
        }

        public abstract void Load(T instance, DateTime dateTime);

        public abstract string FormattedDayHours(DateTime dateTime);

        public DateTime ClosingDateTime(DateTime dateTime)
        {
            return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, ClosingHour, 0, 0, DateTimeKind.Local);
        }

        public DateTime OpeningDateTime(DateTime dateTime)
        {
            return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, OpeningHour, 0, 0, DateTimeKind.Local);
        }

        public bool IsWithinHours(DateTime dateTime)
        {
            var opening = OpeningDateTime(dateTime);
            var closing = ClosingDateTime(dateTime);
            return dateTime >= opening && dateTime <= closing;
        }
    }
}