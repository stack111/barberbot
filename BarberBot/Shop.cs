using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BarberBot
{
    [Serializable]
    public class Shop
    {
        private readonly TimeSpan minimumTimeBeforeClose;
        private readonly IHoursRepository<Barber> barberRepository;

        private StoreHours Hours;

        public Shop(IHoursRepository<Barber> barberRepository)
        {
            Hours = new StoreHours();
            minimumTimeBeforeClose = StoreHours.AppointmentLength;
            this.barberRepository = barberRepository;
        }

        public bool CanAcceptCustomers(DateTime dateTime)
        {
            if (!IsOpen(dateTime))
            {
                return false;
            }
            DateTime nowDateTime = DateTime.UtcNow.AddHours(StoreHours.UTC_to_PST_Hours);
            StoreHours nowHours = new StoreHours();
            nowHours.Load(this, nowDateTime);
            if (dateTime < nowDateTime)
            {
                return false;
            }
            return true;
        }

        public DateTime OpeningDateTime(DateTime dateTime)
        {
            StoreHours hours = new StoreHours();
            hours.Load(this, dateTime);
            return hours.OpeningDateTime();
        }

        public DateTime ClosingDateTime(DateTime dateTime)
        {
            StoreHours hours = new StoreHours();
            hours.Load(this, dateTime);
            return hours.ClosingDateTime();
        }

        public bool IsOpen(DateTime dateTime)
        {
            // todo: special case check for holidays           
            
            Hours.Load(this, dateTime);
            if (!Hours.Exists)
            {
                return false;
            }
            DateTime open = Hours.OpeningDateTime();
            DateTime close = Hours.ClosingDateTime();
            return dateTime >= open && dateTime <= close;
        }

        public async Task<AppointmentAvailabilityResponse> IsAvailableAsync(AppointmentRequest appointmentRequest)
        {
            AppointmentAvailabilityResponse response = new AppointmentAvailabilityResponse();
            var dateTimeToCheck = appointmentRequest.RequestedDateTime.Add(minimumTimeBeforeClose);

            DateTime nowDateTime = DateTime.UtcNow.AddHours(StoreHours.UTC_to_PST_Hours); // convert to PST.
            Hours.Load(this, nowDateTime.Date); // today's hours

            // check holidays

            // is it in the past?
            if (dateTimeToCheck < nowDateTime)
            {
                response.ValidationResults.Add(new ValidationResult()
                {
                    Message = "Appointments can't be scheduled before right now. "
                });
                return response;
            }

            // is the store open?
            Hours.Load(this, dateTimeToCheck);
            if (!Hours.Exists)
            {
                response.ValidationResults.Add(new ValidationResult()
                {
                    Message = "The store isn't open that day. "
                });
                return response;
            }

            // is it between hours
            bool isWithinHours = Hours.IsWithinHours(appointmentRequest.RequestedDateTime);

            // does it meet the minimum window?
            bool meetsMinimum = Hours.IsWithinHours(dateTimeToCheck);
            
            if (isWithinHours && meetsMinimum)
            {
                response.IsAvailable = true;
            }
            else if(!isWithinHours)
            {
                response.ValidationResults.Add(new ValidationResult()
                {
                    Message = $"The appointment isn't between store hours of {FormattedDayHours(appointmentRequest.RequestedDateTime)}. "
                });
            }
            else if(!meetsMinimum)
            {
                response.ValidationResults.Add(new ValidationResult()
                {
                    Message = $"The appointment needs to be at least {StoreHours.AppointmentLength.TotalMinutes} minutes before closing. Hours are {FormattedDayHours(appointmentRequest.RequestedDateTime)}. "
                });
            }
            return response;
        }

        public async Task<BarberAvailabilityResponse> AnyBarbersAvailableAsync(AppointmentRequest appointmentRequest)
        {
            var barbers = LoadBarbers(false);
            BarberAvailabilityResponse availabilityResponse = null;
            foreach (var barber in barbers)
            {
                availabilityResponse = await barber.IsAvailableAsync(appointmentRequest);
                if (availabilityResponse.IsAvailable)
                {
                    return availabilityResponse;
                }
            }
            availabilityResponse = new BarberAvailabilityResponse()
            {
                IsAvailable = false
            };
            availabilityResponse.ValidationResults.Add(new ValidationResult() { Message = $"No barbers are available at {appointmentRequest.RequestedDateTime.ToString()}. " });
            return availabilityResponse;
        }

        public async Task<AppointmentRequest> NextAvailableBarberAsync(AppointmentRequest appointmentRequest)
        {
            var barbers = LoadBarbers(false);

            Barber nextAvailable = null;
            AppointmentRequest nextRequest = new AppointmentRequest(this);
            nextRequest.CopyFrom(appointmentRequest);
            
            int attempts = 0;
            while (nextAvailable == null && attempts < 5)
            {
                foreach (var barber in barbers)
                {
                    BarberAvailabilityResponse availabilityResponse = await barber.IsAvailableAsync(nextRequest);
                    if (availabilityResponse.IsAvailable)
                    {
                        nextAvailable = barber;
                        break;
                    }
                }

                if (nextAvailable == null)
                {
                    DateTime nextDateTimeCheck = nextRequest.RequestedDateTime.Add(BarberHours.AppointmentMiddleLength);
                    if (!IsOpen(nextDateTimeCheck))
                    {
                        nextDateTimeCheck = nextDateTimeCheck.AddDays(1);
                        Hours.Load(this, nextDateTimeCheck);
                        nextDateTimeCheck = Hours.OpeningDateTime();
                    }
                    nextRequest = new AppointmentRequest(this)
                    {
                        RequestedBarber = nextRequest.RequestedBarber,
                        RequestedDateTime = nextDateTimeCheck,
                    };
                    attempts++;
                }
            }

            nextRequest.RequestedBarber = nextAvailable;

            return nextRequest;
        }
        public string FormattedWeekHours(DateTime dateTime)
        {
            Hours.Load(this, dateTime);
            return Hours.FormattedWeekHours();
        }

        public string FormattedDayHours(DateTime dateTime)
        {
            Hours.Load(this, dateTime);
            return Hours.FormattedDayHours();
        }

        public List<Barber> LoadBarbers(bool withAnyone)
        {
            List<Barber> barbers = new List<Barber>()
            {
                new Barber(this, barberRepository) { DisplayName = "Jessica" }
            };
            if (withAnyone)
            {
                barbers.Add(new Barber(this, barberRepository) { DisplayName = "Anyone" });
            }
            return barbers;
        }
    }
}