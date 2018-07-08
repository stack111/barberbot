

namespace BarberBot.Models
{
    using System;
    using System.Runtime.Serialization;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder.Dialogs;

    [Serializable]
    public class BarberDialog : IDialog
    {
        [IgnoreDataMember]
        readonly Shop shop;

        public BarberDialog(Shop shop)
        {
            this.shop = shop;
        }

        private string barberName = string.Empty;
        public async Task StartAsync(IDialogContext context)
        {
             PromptDialog.Choice(
                context,
                this.OnBarberChoiceSelected,
                new string[] { "Jessica", "Anyone" },
                "Ok, can you tell me who's your barber?",
                "I'm sorry but I didn't understand that. can you select one of the barber options below?",
                2, PromptStyle.Auto);

        }

        private async Task OnBarberChoiceSelected(IDialogContext context, IAwaitable<string> result)
        {
            try
            {
                var barberName = await result;

                if (barberName != null)
                {
                    // retrieve barber schedule and the next available window and barbers in that window
                    // suggest a good window of time and day.
                    // todo make this a real suggestion based on calendar.
                    var now = DateTime.Now;
                    var suggestion = new DateTime(now.Year, now.Month, now.Day, now.AddHours(1).Hour, 0, 0);
                    await context.PostAsync($"The barber you requested is: {barberName}, he or she looks open at {string.Format("{0:m} {0:t}", suggestion)}");
                    this.barberName = barberName;


                    var promptAppointmentDateTime = new PromptAppointmentDateTime(shop,
                        "Please enter the date and time of the appointment:",
                        "Sorry I couldn't recognize the date or the time of the requested appointment:",
                        "You have tried to request an appointment too many times, try again briefly.",
                        attempts: 3);

                    context.Call(promptAppointmentDateTime, this.AfterAppointmentChoosen);
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

        private async Task AfterAppointmentChoosen(IDialogContext context, IAwaitable<DateTime> result)
        {
            try
            {
                var appointmentDateTime = await result;

                if (appointmentDateTime != DateTime.MinValue)
                {
                    string formatedDateTime = string.Format("{0:m} {0:t}", appointmentDateTime);

                    // validate the date and time are available for the barber.

                    await context.PostAsync($"The requested appointment with {barberName} is: {formatedDateTime}");


                    context.Done(true);
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
    }

}