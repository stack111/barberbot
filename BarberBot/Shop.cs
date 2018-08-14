using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BarberBot
{
    [Serializable]
    public class Shop : IShop
    {
        private readonly TimeSpan minimumTimeBeforeClose;
        private readonly IRepository<Appointment> appointmentRepository;
        private readonly IBarbersRepository barbersRepository;
        private readonly ShopHours Hours;
        public List<Barber> Barbers { get; set; }
        public HoursType Type => HoursType.Shop;

        public Shop(IRepository<Appointment> appointmentRepository, ShopHours hours, IBarbersRepository barbersRepository)
        {
            Hours = hours;
            minimumTimeBeforeClose = ShopHours.AppointmentMiddleLength;
            this.appointmentRepository = appointmentRepository;
            this.barbersRepository = barbersRepository;
            Barbers = new List<Barber>();
        }

        public async Task<bool> CanAcceptCustomersAsync(DateTime dateTime)
        {
            if (!await IsOpenAsync(dateTime))
            {
                return false;
            }

            DateTime nowDateTime = DateTime.UtcNow.AddHours(ShopHours.UTC_to_PST_Hours);
            ShopHours nowHours = new ShopHours(Hours);
            await nowHours.LoadAsync(this, nowDateTime);
            if (dateTime < nowDateTime)
            {
                return false;
            }
            return true;
        }

        public async Task<DateTime> OpeningDateTimeAsync(DateTime dateTime)
        {
            await Hours.LoadAsync(this, dateTime);
            return Hours.OpeningDateTime();
        }

        public async Task<DateTime> ClosingDateTimeAsync(DateTime dateTime)
        {
            await Hours.LoadAsync(this, dateTime);
            return Hours.ClosingDateTime();
        }

        public async Task<bool> IsOpenAsync(DateTime dateTime)
        {   
            return await Hours.IsAvailableAsync(this, dateTime);
        }

        public async Task<AppointmentAvailabilityResponse> IsAvailableAsync(AppointmentRequest appointmentRequest)
        {
            AppointmentAvailabilityResponse response = new AppointmentAvailabilityResponse();
            var dateTimeToCheck = appointmentRequest.StartDateTime.Add(minimumTimeBeforeClose);

            DateTime nowDateTime = DateTime.UtcNow.AddHours(ShopHours.UTC_to_PST_Hours); // convert to PST.
            await Hours.LoadAsync(this, nowDateTime.Date); // today's hours

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
            await Hours.LoadAsync(this, dateTimeToCheck);
            if (!Hours.Exists)
            {
                response.ValidationResults.Add(new ValidationResult()
                {
                    Message = "The shop isn't open that day. "
                });
                return response;
            }

            // is it between hours
            bool isWithinHours = Hours.IsWithinHours(appointmentRequest.StartDateTime);

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
                    Message = $"The appointment isn't between store hours of {Hours.FormattedDayHours()}. "
                });
            }
            else if(!meetsMinimum)
            {
                response.ValidationResults.Add(new ValidationResult()
                {
                    Message = $"The appointment needs to be at least {minimumTimeBeforeClose} minutes before closing. Hours are {Hours.FormattedDayHours()}. "
                });
            }
            return response;
        }

        public async Task<BarberAvailabilityResponse> AnyBarbersAvailableAsync(AppointmentRequest appointmentRequest)
        {
            var barbers = await LoadBarbersAsync(false);
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
            availabilityResponse.ValidationResults.Add(new ValidationResult() { Message = $"No barbers are available at {appointmentRequest.StartDateTime.ToString()}. " });
            return availabilityResponse;
        }

        public async Task<AppointmentRequest> NextAvailableBarberAsync(AppointmentRequest appointmentRequest)
        {
            var barbers = await LoadBarbersAsync(false);

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
                    DateTime nextDateTimeCheck = nextRequest.StartDateTime.Add(BarberHours.AppointmentMiddleLength);
                    if (!await IsOpenAsync(nextDateTimeCheck))
                    {
                        nextDateTimeCheck = nextDateTimeCheck.AddDays(1);
                        await Hours.LoadAsync(this, nextDateTimeCheck);
                        nextDateTimeCheck = Hours.OpeningDateTime();
                    }
                    nextRequest = new AppointmentRequest(this);
                    nextRequest.CopyFrom(appointmentRequest); 
                    attempts++;
                }
            }

            if(nextAvailable == null)
            {
                // if we can't find a barber this is not a valid request.
                nextRequest = null;
            }
            else
            {
                nextRequest.RequestedBarber = nextAvailable;
            }

            return nextRequest;
        }

        public async Task<string> FormattedWeekHoursAsync(DateTime dateTime)
        {
            await Hours.LoadAsync(this, dateTime);
            return Hours.FormattedWeekHours();
        }

        public async Task<List<Barber>> LoadBarbersAsync(bool withAnyone)
        {
            await barbersRepository.LoadAsync(this, withAnyone);
            return new List<Barber>(Barbers);
        }

        public void LoadFrom(Hours<ISchedulable> hours)
        {
            Hours.ClosingHour = hours.ClosingHour;
            Hours.OpeningHour = hours.OpeningHour;
        }
    }
}