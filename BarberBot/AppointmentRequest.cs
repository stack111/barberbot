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
        private bool roundedRequestedDateTime;

        public AppointmentRequest(Shop shop)
        {
            Shop = shop;
        }

        public DateTime RequestedDateTime { get; set; }

        public Barber RequestedBarber { get; set; }

        public Shop Shop { get; private set; }

        public async Task<AppointmentAvailabilityResponse> IsAvailableAsync()
        {
            RequestedDateTime = await RoundAppointmentDateTimeAsync(RequestedDateTime);
            
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
                    RoundedRequestedTime = roundedRequestedDateTime,
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
                    RoundedRequestedTime = roundedRequestedDateTime,
                    SuggestedRequest = suggestedRequest
                };
                response.ValidationResults.AddRange(barberResponse.ValidationResults);
                return response;
            }
            else
            {
                // shop is not available
                // show the hours for the week and suggest the next available appointment
                DateTime nowDateTime = DateTime.UtcNow.AddHours(StoreHours.UTC_to_PST_Hours);
                AppointmentRequest nextRequest = null;
                AppointmentAvailabilityResponse response = new AppointmentAvailabilityResponse()
                {
                    IsAvailable = false,
                    RoundedRequestedTime = roundedRequestedDateTime
                };
                if (!shopResponse.IsAvailable)
                {
                    nextRequest = new AppointmentRequest(Shop);
                    nextRequest.CopyFrom(this);
                    if (nextRequest.RequestedDateTime < nowDateTime)
                    {
                        int attempts = 0;
                        while (!Shop.CanAcceptCustomers(nextRequest.RequestedDateTime) && attempts < 5)
                        {
                            nextRequest.RequestedDateTime = nextRequest.RequestedDateTime.AddDays(1);
                            nextRequest.RequestedDateTime = Shop.OpeningDateTime(nextRequest.RequestedDateTime);
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
                    if (nextRequest.RequestedDateTime < nowDateTime)
                    {
                        int attempts = 0;
                        nextRequest.RequestedDateTime = nextRequest.RequestedDateTime.AddDays(1);
                        RequestedBarber.Hours.Load(RequestedBarber, nextRequest.RequestedDateTime);
                        while (!Shop.IsOpen(nextRequest.RequestedDateTime) && !RequestedBarber.Hours.IsWithinHours(nextRequest.RequestedDateTime) && attempts < 5)
                        {
                            nextRequest.RequestedDateTime = nextRequest.RequestedDateTime.AddDays(1);
                            nextRequest.RequestedDateTime = Shop.OpeningDateTime(nextRequest.RequestedDateTime);
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
            RequestedDateTime = DateTime.MinValue;
        }

        public string ToSuggestionString()
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendFormat("{2} {0:m} at {0:t} with {1}", RequestedDateTime, RequestedBarber.DisplayName, RequestedDateTime.ToString("ddd"));
            return builder.ToString();
        }

        private async Task<DateTime> RoundAppointmentDateTimeAsync(DateTime requestedTime)
        {
            DateTime newProposedTime = new DateTime(requestedTime.Year, requestedTime.Month, requestedTime.Day, requestedTime.Hour, requestedTime.Minute, 0, requestedTime.Kind);
            int hour = requestedTime.Hour;
            int minute = requestedTime.Minute;
            roundedRequestedDateTime = true;
            if (requestedTime.Minute > 0 && requestedTime.Minute <= 15)
            {
                newProposedTime = newProposedTime.AddMinutes(-1 * minute);
            }
            else if (requestedTime.Minute > 15 && requestedTime.Minute <= 29)
            {
                var roundMark = 30 - minute;
                newProposedTime = newProposedTime.AddMinutes(roundMark);
            }
            else if (requestedTime.Minute >= 31 && requestedTime.Minute <= 45)
            {
                var roundMark = minute - 30;
                newProposedTime = newProposedTime.AddMinutes(-1 * roundMark);
            }
            else if (requestedTime.Minute > 45 && requestedTime.Minute <= 59)
            {
                var roundMark = 60 - minute;
                newProposedTime = newProposedTime.AddMinutes(roundMark);
            }
            else
            {
                roundedRequestedDateTime = false;
            }

            return newProposedTime;
        }

        public void CopyFrom(AppointmentRequest suggestedAppointmentRequest)
        {
            Shop = suggestedAppointmentRequest.Shop;
            RequestedDateTime = suggestedAppointmentRequest.RequestedDateTime;
            RequestedBarber = suggestedAppointmentRequest.RequestedBarber;
        }
    }
}