using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BarberBot
{
    [Serializable]
    public class AppointmentRequestDateTimeRounding
    {
        public async Task<DateTime> RoundDateTimeAsync(DateTime requestedTime)
        {
            DateTime newProposedTime = new DateTime(requestedTime.Year, requestedTime.Month, requestedTime.Day, requestedTime.Hour, requestedTime.Minute, 0, requestedTime.Kind);
            int hour = requestedTime.Hour;
            int minute = requestedTime.Minute;
            if (requestedTime.Minute > 0 && requestedTime.Minute <= 5)
            {
                newProposedTime = newProposedTime.AddMinutes(-1 * minute);
            }
            else if (requestedTime.Minute > 5 && requestedTime.Minute <= 9)
            {
                var roundMark = 10 - minute;
                newProposedTime = newProposedTime.AddMinutes(roundMark);
            }
            else if (requestedTime.Minute > 10 && requestedTime.Minute <= 13)
            {
                var roundMark = 10 - minute;
                newProposedTime = newProposedTime.AddMinutes(roundMark);
            }
            else if (requestedTime.Minute > 13 && requestedTime.Minute <= 14)
            {
                var roundMark = 15 - minute;
                newProposedTime = newProposedTime.AddMinutes(roundMark);
            }
            else if (requestedTime.Minute > 15 && requestedTime.Minute <= 17)
            {
                var roundMark = 15 - minute;
                newProposedTime = newProposedTime.AddMinutes(roundMark);
            }
            else if (requestedTime.Minute > 17 && requestedTime.Minute < 20)
            {
                var roundMark = 20 - minute;
                newProposedTime = newProposedTime.AddMinutes(roundMark);
            }
            else if (requestedTime.Minute > 20 && requestedTime.Minute <= 25)
            {
                var roundMark = 20 - minute;
                newProposedTime = newProposedTime.AddMinutes(roundMark);
            }
            else if (requestedTime.Minute > 25 && requestedTime.Minute <= 29)
            {
                var roundMark = 30 - minute;
                newProposedTime = newProposedTime.AddMinutes(roundMark);
            }
            else if (requestedTime.Minute >= 31 && requestedTime.Minute <= 35)
            {
                var roundMark = 30 - minute;
                newProposedTime = newProposedTime.AddMinutes(roundMark);
            }
            else if (requestedTime.Minute > 35 && requestedTime.Minute <= 39)
            {
                var roundMark = 40 - minute;
                newProposedTime = newProposedTime.AddMinutes(roundMark);
            }
            else if (requestedTime.Minute >= 41 && requestedTime.Minute <= 43)
            {
                var roundMark = 40 - minute;
                newProposedTime = newProposedTime.AddMinutes(roundMark);
            }
            else if (requestedTime.Minute > 43 && requestedTime.Minute <= 44)
            {
                var roundMark = 45 - minute;
                newProposedTime = newProposedTime.AddMinutes(roundMark);
            }
            else if (requestedTime.Minute > 45 && requestedTime.Minute <= 47)
            {
                var roundMark = 45 - minute;
                newProposedTime = newProposedTime.AddMinutes(roundMark);
            }
            else if (requestedTime.Minute > 47 && requestedTime.Minute <= 49)
            {
                var roundMark = 50 - minute;
                newProposedTime = newProposedTime.AddMinutes(roundMark);
            }
            else if (requestedTime.Minute > 51 && requestedTime.Minute <= 55)
            {
                var roundMark = 50 - minute;
                newProposedTime = newProposedTime.AddMinutes(roundMark);
            }
            else if (requestedTime.Minute > 55 && requestedTime.Minute <= 59)
            {
                var roundMark = 60 - minute;
                newProposedTime = newProposedTime.AddMinutes(roundMark);
            }

            return newProposedTime;
        }
    }
}