using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace BarberBot
{
    [Serializable]
    [JsonObject]
    public class Barber
    {
        private readonly Shop shop;
        private readonly IRepository<Barber> repository;

        public Barber(Shop shop, IRepository<Barber> repository)
        {
            this.shop = shop;
            this.repository = repository;
        }

        [JsonProperty(PropertyName = "displayName")]
        public string DisplayName { get; set; }

        public BarberHours Hours { get; internal set; }

        public async Task<BarberAvailabilityResponse> IsAvailableAsync(AppointmentRequest appointmentRequest)
        {
            if (string.Equals(DisplayName, "Anyone", StringComparison.OrdinalIgnoreCase))
            {
                return await shop.AnyBarbersAvailableAsync(appointmentRequest);
            }

            // check working days / hours
            // check if already reserved
            await repository.LoadHoursAsync(this, appointmentRequest.RequestedDateTime);
            bool available = await repository.IsAppointmentAvailableAsync(this, appointmentRequest.RequestedDateTime);
            bool barberAvailability = Hours.Exists && available;
            // if not available get next available barber
            BarberAvailabilityResponse availabilityResponse = new BarberAvailabilityResponse() { IsAvailable = barberAvailability, Barber = this };

            if (!Hours.Exists || !available)
            {
                availabilityResponse.ValidationResults.Add(new ValidationResult() { Message = $"{DisplayName} is not available on this date or time." });
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

            // look at working days / hours 
            await repository.LoadHoursAsync(this, appointmentRequest.RequestedDateTime);
            bool available = await repository.IsAppointmentAvailableAsync(this, appointmentRequest.RequestedDateTime);
            // create a request based on the incoming request with the right datetime, shop, and barber.
            AppointmentRequest suggestedAppointment = new AppointmentRequest(shop);
            suggestedAppointment.CopyFrom(appointmentRequest);

            if (Hours.Exists && available)
            {
                return suggestedAppointment;
            }
            else
            {
                int attempts = 0;
                DateTime nextDateTimeCheck = suggestedAppointment.RequestedDateTime;
                while (!Hours.Exists && !available)
                {
                    nextDateTimeCheck = suggestedAppointment.RequestedDateTime.AddHours(1);
                    if (!shop.IsOpen(nextDateTimeCheck))
                    {
                        nextDateTimeCheck = nextDateTimeCheck.AddDays(1);
                        nextDateTimeCheck = shop.Hours.OpeningDateTime(nextDateTimeCheck);
                    }

                    Hours.Load(this, nextDateTimeCheck);
                    available = await repository.IsAppointmentAvailableAsync(this, appointmentRequest.RequestedDateTime);
                    attempts++;
                    if(attempts > 5)
                    {
                        break;
                    }
                }

                if(attempts < 5 && Hours.Exists && available)
                {
                    suggestedAppointment.RequestedDateTime = nextDateTimeCheck;
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
    }
}