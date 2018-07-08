using System;

namespace BarberBot
{
    [Serializable]
    public class Shop
    {
        private readonly StoreHours storeHours;
        private readonly TimeSpan minimumTimeBeforeClose;
        private const int UTC_to_PST_Hours = -7;
        public Shop()
        {
            storeHours = new StoreHours();
            minimumTimeBeforeClose = TimeSpan.FromHours(1);
        }

        public bool IsShopOpen(DateTime dateTime, bool withReservationMinimumTime = false)
        {
            // todo: special case check for holidays           
            storeHours.Load(dateTime);
            if (!storeHours.Exists)
            {
                return false;
            }
            DateTime open = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, storeHours.OpeningHour, 0, 0, 0, dateTime.Kind);
            DateTime close = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, storeHours.ClosingHour, 0, 0, 0, dateTime.Kind);
            return dateTime >= open && dateTime <= close;
        }

        public AppointmentValidationResponse CanMakeAppointment(DateTime dateTime)
        {
            AppointmentValidationResponse response = new AppointmentValidationResponse();
            var dateTimeToCheck = dateTime.Add(minimumTimeBeforeClose);

            DateTime nowDateTime = DateTime.UtcNow.AddHours(UTC_to_PST_Hours); // convert to PST.
            nowDateTime = new DateTime(nowDateTime.Year, nowDateTime.Month, nowDateTime.Day);
            storeHours.Load(nowDateTime); // today's hours

            // check holidays

            // is it in the past?
            if (dateTimeToCheck < storeHours.ClosingDateTime(nowDateTime))
            {
                response.ValidationResults.Add(new ValidationResult()
                {
                    Message = "Appointments can't be scheduled in the past. "
                });
                return response;
            }

            // is the store open?
            storeHours.Load(dateTimeToCheck);
            if (!storeHours.Exists)
            {
                response.ValidationResults.Add(new ValidationResult()
                {
                    Message = "The store isn't open that day. "
                });
                return response;
            }

            // is it between hours
            bool isWithinHours = storeHours.IsWithinHours(dateTime);

            // does it meet the minimum window?
            bool meetsMinimum = storeHours.IsWithinHours(dateTimeToCheck);
            
            if (isWithinHours && meetsMinimum)
            {
                response.IsValid = true;
            }
            else if(!isWithinHours)
            {
                response.ValidationResults.Add(new ValidationResult()
                {
                    Message = $"The appointment isn't between store hours of {FormattedDayHours(dateTime)} "
                });
            }
            else if(!meetsMinimum)
            {
                response.ValidationResults.Add(new ValidationResult()
                {
                    Message = $"The appointment at least 1 hour before closing, hours today are: {FormattedDayHours(dateTime)} "
                });
            }
            return response;
        }

        public string FormattedWeekHours(DateTime dateTime)
        {
            return storeHours.FormattedWeekHours(dateTime);
        }

        public string FormattedDayHours(DateTime dateTime)
        {
            return storeHours.FormattedDayHours(dateTime);
        }
    }
}