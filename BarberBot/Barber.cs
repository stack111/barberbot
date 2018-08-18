using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BarberBot
{
    [Serializable]
    [JsonObject]
    public class Barber : IBarber
    {
        private readonly IShop shop;
        private readonly IRepository<Appointment> appointmentRepository;

        public Barber(IShop shop, IRepository<Appointment> appointmentRepository, Hours<IBarber> hours)
        {
            this.shop = shop;
            this.appointmentRepository = appointmentRepository;
            Hours = hours;
        }

        [JsonProperty(PropertyName = "displayName")]
        public string DisplayName { get; set; }

        public Hours<IBarber> Hours { get; private set; }

        public HoursType Type => HoursType.Barber;

        public async Task<List<BarberService>> LoadServicesAsync()
        {
            return new List<BarberService>()
            {
                new BarberService()
                {
                    DisplayName = "Buzz Cut",
                    Duration = TimeSpan.FromMinutes(15),
                    Description = "Trimmer for hair, no scissors or shears or special services."
                },
                new BarberService()
                {
                    DisplayName = "Short Hair Cut",
                    Duration = TimeSpan.FromMinutes(30),
                    Description = "Shears, scissors, trimmers, or anything needed to get the hair you want."
                },
                new BarberService()
                {
                    DisplayName = "Long Hair Cut",
                    Duration = TimeSpan.FromMinutes(40),
                    Description = "Maybe you want a steamed towel or other extras. This includes anything needed to get the hair you want."
                }
            };
        }

        public async Task<BarberAvailabilityResponse> IsAvailableAsync(AppointmentRequest appointmentRequest)
        {
            if (string.Equals(DisplayName, "Anyone", StringComparison.OrdinalIgnoreCase))
            {
                return await shop.AnyBarbersAvailableAsync(appointmentRequest);
            }

            // check working days / hours
            // check if already reserved
            bool startTimeAvailable = await Hours.IsAvailableAsync(this, appointmentRequest.StartDateTime);

            DateTime durationTime = appointmentRequest.StartDateTime.Add(appointmentRequest.Service.Duration);
            bool durationAvailable = await Hours.IsAvailableAsync(this, durationTime);
            Appointment appointment = new Appointment(appointmentRepository);
            appointment.CopyFrom(appointmentRequest);
            bool conflictingAppointment = await appointment.ExistsAsync();
            bool barberAvailability = Hours.Exists && startTimeAvailable && !conflictingAppointment;
            // if not available get next available barber
            BarberAvailabilityResponse availabilityResponse = new BarberAvailabilityResponse()
            {
                IsAvailable = barberAvailability,
                IsConflictingAppointment = conflictingAppointment,
                Barber = this
            };

            if (!barberAvailability)
            {
                availabilityResponse.ValidationResults.Add(new ValidationResult() { Message = $"{DisplayName} is not available on this date or time. " });
            }

            return availabilityResponse;
        }

        public async Task<AppointmentRequest> NextAvailableRequestAsync(AppointmentRequest appointmentRequest)
        {
            // special case for "Anyone"
            if(string.Equals(DisplayName, "Anyone", StringComparison.OrdinalIgnoreCase))
            {
                return await shop.NextAvailableBarberAsync(appointmentRequest);
            }

            BarberAvailabilityResponse availableResponse = await IsAvailableAsync(appointmentRequest);
            // create a request based on the incoming request with the right datetime, shop, and barber.
            AppointmentRequest suggestedAppointment = new AppointmentRequest(shop);
            suggestedAppointment.CopyFrom(appointmentRequest);

            if (availableResponse.IsAvailable)
            {
                return suggestedAppointment;
            }
            else
            {
                int attempts = 0;
                DateTime nextDateTimeCheck = suggestedAppointment.StartDateTime;
                while (!availableResponse.IsAvailable && attempts < BarberHours.SuggestionAttempts)
                {
                    nextDateTimeCheck = suggestedAppointment.StartDateTime.Add(BarberHours.SuggestionAttemptIncrement);
                    if (!await shop.IsOpenAsync(nextDateTimeCheck))
                    {
                        nextDateTimeCheck = nextDateTimeCheck.AddDays(1);
                        nextDateTimeCheck = await shop.OpeningDateTimeAsync(nextDateTimeCheck);
                    }

                    suggestedAppointment.StartDateTime = nextDateTimeCheck;
                    availableResponse = await IsAvailableAsync(suggestedAppointment);
                    attempts++;
                }

                if(attempts < BarberHours.SuggestionAttempts && availableResponse.IsAvailable)
                {
                    suggestedAppointment.StartDateTime = nextDateTimeCheck;
                }
                else
                {
                    suggestedAppointment = await shop.NextAvailableBarberAsync(appointmentRequest);
                }
            }

            return suggestedAppointment;
        }

        public override string ToString()
        {
            return DisplayName;
        }

        public override bool Equals(object obj)
        {
            if(obj.GetType() != this.GetType())
            {
                return false;
            }

            Barber other = (Barber)obj;
            return string.Equals(other.DisplayName, DisplayName, StringComparison.OrdinalIgnoreCase);
        }

        public override int GetHashCode()
        {
            return DisplayName.GetHashCode();
        }

        public void LoadFrom(Hours<ISchedulable> hours)
        {
            Hours.ClosingHour = hours.ClosingHour;
            Hours.OpeningHour = hours.OpeningHour;
        }
    }
}