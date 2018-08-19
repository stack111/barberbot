using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BarberBot.Dialogs
{
    [Serializable]
    public class RootDialog : IDialog<object>
    {
        private readonly Appointment appointment;
        private readonly Shop shop;
        public RootDialog(Appointment appointment, Shop shop)
        {
            this.appointment = appointment;
            this.shop = shop;
        }

        private Dictionary<string, string> NumberToActionMap = new Dictionary<string, string>()
            {
                { "1", requestAppointmentConst },
                { "2", cancelAppointmentConst }
            };
        
        private const string requestAppointmentConst = "Request Appointment";
        private const string cancelAppointmentConst = "Cancel Appointment";



        public async Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);
        }

        public async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            PromptDialog.Choice(
                context,
                this.AfterChoiceSelected,
                NumberToActionMap.Values,
                "Hi I'm BarberBot, how can I help you?",
                "I'm sorry but I didn't understand that. can you select one of the options below?",
                2, PromptStyle.Auto);
        }

        private async Task AfterChoiceSelected(IDialogContext context, IAwaitable<string> result)
        {
            try
            {
                var selection = await result;
                
                switch (selection)
                {
                    case requestAppointmentConst:
                        context.Call(new AppointmentRequestDialog(appointment, new AppointmentRequest(shop)), AfterApointmentRequestAsync);
                        
                        break;
                    case cancelAppointmentConst:
                        await context.PostAsync("This functionality is not yet done! Try requesting an appointment.");
                        await this.StartAsync(context);
                        break;
                }
            }
            catch (TooManyAttemptsException)
            {
                await this.StartAsync(context);
            }
        }

        private async Task AfterApointmentRequestAsync(IDialogContext context, IAwaitable<object> result)
        {
            var success = (bool) await result;

            if (!success)
            {
                await context.PostAsync("We didn't reserve that appointment, but you can try again.");
            }
            else
            {
                await context.PostAsync("Thanks!");
            }

            await this.StartAsync(context);
        }
    }
}