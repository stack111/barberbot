using System;

namespace BarberBot
{
    [Serializable]
    public class BarberHours : Hours<Barber>
    {
        private DateTime dateTime = DateTime.MinValue;
        public override DateTime DateTime
        {
            get
            {
                return dateTime;
            }
            protected set
            {
                dateTime = value;
            }
        }

        public override void Load(Barber instance, DateTime dateTime)
        {
            // todo: special case check for holidays
            DateTime = dateTime;
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

            OpeningHour = openHour;
            ClosingHour = closeHour;
        }

        public override string FormattedDayHours()
        {
            if (Exists)
            {

                return $"{DateTime.ToString("ddd")} {String.Format("{0:t}", OpeningDateTime())} - {String.Format("{0:t}", ClosingDateTime())}";
            }
            else
            {
                return "N/A";
            }
        }
    }
}