

namespace BarberBot.Models
{
    using System;
    using System.Runtime.Serialization;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder.Dialogs;

    [Serializable]
    public class AppointmentRequestDialog : IDialog
    {
        [IgnoreDataMember]
        readonly AppointmentRequest request;

        [IgnoreDataMember]
        readonly Appointment appointment;

        public AppointmentRequestDialog(Appointment appointment, AppointmentRequest request)
        {
            this.appointment = appointment;
            this.request = request;
        }

        public async Task StartAsync(IDialogContext context)
        {
             PromptDialog.Choice(
                context,
                this.OnBarberChoiceSelected,
                request.Shop.LoadBarbers(true),
                "Ok, can you tell me who's your barber?",
                "I'm sorry but I didn't understand that. can you select one of the barber options below?",
                2, PromptStyle.Auto);

        }

        private async Task OnBarberChoiceSelected(IDialogContext context, IAwaitable<Barber> result)
        {
            try
            {
                var barber = await result;

                if (barber != null)
                { 
                    // retrieve barber schedule and the next available window and barbers in that window
                    // suggest a good window of time and day.
                    // todo make this a real suggestion based on calendar.
                    this.request.RequestedBarber = barber;
                    var promptAppointmentDateTime = new PromptAppointmentRequestDateTime(request,
                        $"Ok can you tell me a date and time you would prefer for your appointment with {barber.DisplayName}?",
                        retry: "You have tried to request an appointment too many times, try again briefly.",
                        attempts: 3);

                    context.Call(promptAppointmentDateTime, this.AfterAppointmentDateTimeChoosen);
                }
                else
                {
                    context.Done(false);
                }
            }
            catch (TooManyAttemptsException)
            {
                context.Done(false);
            }
        }

        private async Task AfterAppointmentDateTimeChoosen(IDialogContext context, IAwaitable<AppointmentRequest> result)
        {
            try
            {
                var appointmentRequest = await result;

                if (appointmentRequest.RequestedDateTime != DateTime.MinValue)
                {
                    var promptTodayConfirmation = new PromptDialog.PromptConfirm(
                               $"Can you confirm your appointment with {appointmentRequest.RequestedBarber} on {string.Format("{0} {1:m} at {1:t}", appointmentRequest.RequestedDateTime.ToString("ddd"), appointmentRequest.RequestedDateTime)}?",
                               "Sorry I didn't understand you. Can you choose an option below?",
                               2);
                    request.CopyFrom(appointmentRequest);
                    context.Call(promptTodayConfirmation, this.AfterAppointmentConfirmation);
                }
                else
                {
                    request.ResetDateTime();
                    context.Done(false);
                }
            }
            catch (TooManyAttemptsException)
            {
                context.Done(false);
            }
        }

        private async Task AfterAppointmentConfirmation(IDialogContext context, IAwaitable<bool> result)
        {
            bool confirmed = await result;
            if (confirmed)
            {
                appointment.CopyFrom(request);
                await appointment.BookAsync();
                await context.PostAsync("Ok great, see you then! As a friendly reminder we have a 15 minute late cancellation for no-shows.", context.Activity.AsMessageActivity().Locale);
                context.Done(true);
            }
            else
            {
                await context.PostAsync("Ok.", context.Activity.AsMessageActivity().Locale);
                context.Done(false);
            }
        }
    }

}