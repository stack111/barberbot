using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace BarberBot
{
    [Serializable]
    public class StoreHours : Hours<Shop>
    {
        public override void Load(Shop instance, DateTime dateTime)
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

        public override string FormattedDayHours(DateTime dateTime)
        {
            if (Exists)
            {
                
                return $"{dateTime.ToString("ddd")} {String.Format("{0:t}", OpeningDateTime(dateTime))} - {String.Format("{0:t}", ClosingDateTime(dateTime))}";
            }
            else
            {
                return "Closed";
            }
        }
    }
}