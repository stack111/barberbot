using System;

namespace BarberBot
{
    [Serializable]
    public class BarberHours : Hours<Barber>
    {
        public override void Load(Barber instance, DateTime dateTime)
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

            OpeningHour = openHour;
            ClosingHour = closeHour;
        }

        public override string FormattedDayHours(DateTime dateTime)
        {
            if (Exists)
            {

                return $"{dateTime.ToString("ddd")} {String.Format("{0:t}", OpeningDateTime(dateTime))} - {String.Format("{0:t}", ClosingDateTime(dateTime))}";
            }
            else
            {
                return "N/A";
            }
        }
    }
}