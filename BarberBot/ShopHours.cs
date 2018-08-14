using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace BarberBot
{
    [Serializable]
    public class ShopHours : Hours<IShop>
    {
        private DateTime dateTime = DateTime.MinValue;

        public ShopHours(IHoursRepository hoursRepository) : base(hoursRepository)
        {
        }

        public ShopHours(Hours<IShop> hours) : base(hours)
        {
        }

        public string FormattedWeekHours()
        {
            DateTime opening = OpeningDateTime();
            StringBuilder stringBuilder = new StringBuilder();
            
            int beginningEnumCounter = (int)opening.DayOfWeek;
            int daysToSubtract = 0;
            while(beginningEnumCounter > 0)
            {
                daysToSubtract++;
                beginningEnumCounter--;
            }

            for(int days = daysToSubtract; days >= 0; days--)
            {
                ShopHours day = new ShopHours(this);
                DateTime dateTimeCheck = opening.AddDays(-1 * days);
                stringBuilder.AppendLine(day.FormattedDayHours());
            }

            return stringBuilder.ToString();
        }

        protected override string NotAvailableHoursString => "Closed";
    }
}