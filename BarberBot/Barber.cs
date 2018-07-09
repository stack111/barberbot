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

        public Barber(Shop shop)
        {
            this.shop = shop;
        }

        [JsonProperty(PropertyName = "displayName")]
        public string DisplayName { get; set; }

        public async Task<BarberAvailabilityResponse> IsAvailableAsync(AppointmentRequest appointmentRequest)
        {
            if (string.Equals(DisplayName, "Anyone", StringComparison.OrdinalIgnoreCase))
            {
                return await shop.AnyBarbersAvailableAsync(appointmentRequest);
            }

            // check working days / hours
            // check if already reserved

            // if not available get next available barber

            return new BarberAvailabilityResponse() { IsAvailable = true, Barber = this };
        }

        public async Task<AppointmentRequest> NextAvailableRequestAsync(AppointmentRequest appointmentRequest)
        {
            // special case for "Anyone"
            if(string.Equals(DisplayName, "Anyone", StringComparison.OrdinalIgnoreCase))
            {
                return await shop.NextAvailableBarberAsync(appointmentRequest);
            }

            // look at working days / hours 
            // create a request based on the incoming request with the right datetime, shop, and barber.
            
            return new AppointmentRequest(shop) { RequestedBarber = this, RequestedDateTime = DateTime.MaxValue };
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