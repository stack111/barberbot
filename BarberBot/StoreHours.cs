using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace BarberBot
{
    [Serializable]
    public class StoreHours
    {
        public int OpeningHour { get; set; }
        public int ClosingHour { get; set; }

        public bool Exists
        {
            get
            {
                return !(OpeningHour == 0 && ClosingHour == 0);
            }
        }

        public void Load(DateTime dateTime)
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

            OpeningHour = openHour;
            ClosingHour = closeHour;
        }

        public DateTime ClosingDateTime(DateTime dateTime)
        {
            Load(dateTime);
            return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, ClosingHour, 0, 0, DateTimeKind.Local);
        }

        public DateTime OpeningDateTime(DateTime dateTime)
        {
            Load(dateTime);
            return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, OpeningHour, 0, 0, DateTimeKind.Local);
        }

        public bool IsWithinHours(DateTime dateTime)
        {
            var opening = OpeningDateTime(dateTime);
            var closing = ClosingDateTime(dateTime);
            return dateTime >= opening && dateTime <= closing;
        }

        public string FormattedWeekHours(DateTime dateTime)
        {
            DateTime date = dateTime.Date;
            StringBuilder stringBuilder = new StringBuilder();
            

            int beginningEnumCounter = (int)date.DayOfWeek;
            int daysToSubtract = 0;
            while(beginningEnumCounter > 0)
            {
                daysToSubtract++;
                beginningEnumCounter--;
            }

            for(int days = daysToSubtract; days >= 0; days--)
            {
                StoreHours day = new StoreHours();
                DateTime dateTimeCheck = date.AddDays(-1 * days);
                stringBuilder.AppendLine(day.FormattedDayHours(dateTimeCheck));
            }

            return stringBuilder.ToString();
        }

        public string FormattedDayHours(DateTime dateTime)
        {
            Load(dateTime);
            if (Exists)
            {
                DateTime opening = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, OpeningHour, 0, 0);
                DateTime closing = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, ClosingHour, 0, 0);
                return $"{dateTime.ToString("ddd")}: {String.Format("{0:t}", opening)} - {String.Format("{0:t}", closing)}";
            }
            else
            {
                return "Closed";
            }
        }
    }
}