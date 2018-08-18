using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace BarberBot
{
    [Serializable]
    public class AppointmentRequest
    {
        private readonly AppointmentRequestDateTimeRounding roundingRules;
        public AppointmentRequest(IShop shop)
        {
            Shop = shop;
            roundingRules = new AppointmentRequestDateTimeRounding();
        }

        public DateTime StartDateTime { get; set; }

        public IBarber RequestedBarber { get; set; }

        public IShop Shop { get; private set; }

        public BarberService Service { get; set; }

        public async Task<AppointmentAvailabilityResponse> IsAvailableAsync()
        {
            DateTime nowDateTime = DateTime.UtcNow.AddHours(ShopHours.UTC_to_PST_Hours);

            if (StartDateTime < nowDateTime)
            {
                var response = new AppointmentAvailabilityResponse()
                {
                    IsAvailable = false
                };
                response.ValidationResults.Add(new ValidationResult() { Message = "That date and time is in the past." });
                return response;
            }

            StartDateTime = await roundingRules.RoundDateTimeAsync(StartDateTime);
            TimeSpan difference = nowDateTime - StartDateTime;

            // we need to adjust the rounding for any rounding to a few minutes in the past.
            while (StartDateTime < nowDateTime && difference < TimeSpan.FromMinutes(1))
            {
                StartDateTime = await roundingRules.RoundDateTimeAsync(StartDateTime.AddMinutes(5));
            }

            var shopResponse = await Shop.IsAvailableAsync(this);
            BarberAvailabilityResponse barberResponse = await RequestedBarber.IsAvailableAsync(this);
            if (RequestedBarber != barberResponse.Barber)
            {
                // this happens if the user requests "Anyone"
                RequestedBarber = barberResponse.Barber;
            }

            if (shopResponse.IsAvailable && barberResponse.IsAvailable)
            {
                return new AppointmentAvailabilityResponse()
                {
                    IsAvailable = true,
                    SuggestedRequest = this
                };
            }
            else if(shopResponse.IsAvailable && !barberResponse.IsAvailable)
            {
                // barber is not available
                // we could either suggest the next available barber's time
                // and we could find when the barber is available next
                AppointmentRequest suggestedRequest = await RequestedBarber.NextAvailableRequestAsync(this);
                var response = new AppointmentAvailabilityResponse()
                {
                    IsAvailable = false,
                    SuggestedRequest = suggestedRequest
                };
                response.ValidationResults.AddRange(barberResponse.ValidationResults);
                return response;
            }
            else
            {
                // shop is not available
                // show the hours for the week and suggest the next available appointment
                
                AppointmentRequest nextRequest = null;
                AppointmentAvailabilityResponse response = new AppointmentAvailabilityResponse()
                {
                    IsAvailable = false
                };
                if (!shopResponse.IsAvailable)
                {
                    nextRequest = new AppointmentRequest(Shop);
                    nextRequest.CopyFrom(this);
                    if (nextRequest.StartDateTime < nowDateTime)
                    {
                        int attempts = 0;
                        while (!await Shop.CanAcceptCustomersAsync(nextRequest.StartDateTime) && attempts < 5)
                        {
                            nextRequest.StartDateTime = nextRequest.StartDateTime.AddDays(1);
                            nextRequest.StartDateTime = await Shop.OpeningDateTimeAsync(nextRequest.StartDateTime);
                            attempts++;
                        }
                    }
                    nextRequest = await Shop.NextAvailableBarberAsync(nextRequest);
                    response.ValidationResults.AddRange(shopResponse.ValidationResults);
                }
                else if (!barberResponse.IsAvailable)
                {
                    nextRequest = new AppointmentRequest(Shop);
                    nextRequest.CopyFrom(this);
                    if (nextRequest.StartDateTime < nowDateTime)
                    {
                        int attempts = 0;
                        nextRequest.StartDateTime = nextRequest.StartDateTime.AddDays(1);
                        await RequestedBarber.Hours.LoadAsync(RequestedBarber, nextRequest.StartDateTime);
                        while (!await Shop.IsOpenAsync(nextRequest.StartDateTime) && !RequestedBarber.Hours.IsWithinHours(nextRequest.StartDateTime) && attempts < 5)
                        {
                            nextRequest.StartDateTime = nextRequest.StartDateTime.AddDays(1);
                            nextRequest.StartDateTime = await Shop.OpeningDateTimeAsync(nextRequest.StartDateTime);
                            attempts++;
                        }
                    }
                    nextRequest = await RequestedBarber.NextAvailableRequestAsync(nextRequest);
                    response.ValidationResults.AddRange(barberResponse.ValidationResults);
                }

                response.SuggestedRequest = nextRequest;
                return response;
            }
        }

        public void ResetDateTime()
        {
            StartDateTime = DateTime.MinValue;
        }

        public string ToSuggestionString()
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendFormat("{2} {0:m} at {0:t} with {1}", StartDateTime, RequestedBarber.DisplayName, StartDateTime.ToString("ddd"));
            return builder.ToString();
        }

        public void CopyFrom(AppointmentRequest suggestedAppointmentRequest)
        {
            Shop = suggestedAppointmentRequest.Shop;
            StartDateTime = suggestedAppointmentRequest.StartDateTime;
            RequestedBarber = suggestedAppointmentRequest.RequestedBarber;
            Service = suggestedAppointmentRequest.Service;
        }
    }
}