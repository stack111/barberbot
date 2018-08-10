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
        private readonly IHoursRepository<Barber> hoursRepository;
        private readonly IRepository<Appointment> appointmentRepository;
        public Barber(Shop shop, IHoursRepository<Barber> hoursRespository, IRepository<Appointment> appointmentRepository)
        {
            this.shop = shop;
            this.hoursRepository = hoursRespository;
            this.appointmentRepository = appointmentRepository;
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
            bool available = await hoursRepository.IsAvailableAsync(this, appointmentRequest.StartDateTime);
            Appointment appointment = new Appointment(appointmentRepository);
            appointment.CopyFrom(appointmentRequest);
            bool conflictingAppointment = await appointment.ExistsAsync();
            bool barberAvailability = Hours.Exists && available && !conflictingAppointment;
            bool withinHours = Hours.IsWithinHours(appointmentRequest.StartDateTime);
            // if not available get next available barber
            BarberAvailabilityResponse availabilityResponse = new BarberAvailabilityResponse()
            {
                IsAvailable = barberAvailability,
                IsWithinHours = withinHours,
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
                while (!availableResponse.IsAvailable && attempts < 5)
                {
                    nextDateTimeCheck = suggestedAppointment.StartDateTime.Add(BarberHours.AppointmentMiddleLength);
                    if (!shop.IsOpen(nextDateTimeCheck))
                    {
                        nextDateTimeCheck = nextDateTimeCheck.AddDays(1);
                        nextDateTimeCheck = shop.OpeningDateTime(nextDateTimeCheck);
                    }

                    suggestedAppointment.StartDateTime = nextDateTimeCheck;
                    availableResponse = await IsAvailableAsync(suggestedAppointment);
                    attempts++;
                }

                if(attempts < 5 && availableResponse.IsAvailable)
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
    }
}