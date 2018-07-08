namespace BarberBot.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Builder.Dialogs.Internals;
    using Microsoft.Bot.Connector;
    using Microsoft.Recognizers.Text.DateTime;

    [Serializable]
    public class PromptAppointmentDateTime : Prompt<DateTime, string>
    {
        string timeValue;
        private readonly Shop shop;

        public PromptAppointmentDateTime(Shop shop, string prompt, string retry = null, string tooManyAttempts = null, int attempts = 3)
          : base(new PromptOptions<string>(prompt, retry, tooManyAttempts, attempts: attempts))
        {
            this.shop = shop;
        }

        protected override bool TryParse(IMessageActivity message, out DateTime result)
        {
            result = DateTime.MinValue;
            try
            {
                var quitCondition = message.Text.ToLower().Contains("cancel");

                if (quitCondition)
                {
                    return quitCondition;
                }
                else
                {
                    var models = DateTimeRecognizer.RecognizeDateTime(message.Text, message.Locale);
                    if (models == null || models.Count == 0)
                    {
                        return false;
                    }
                    string data = string.Empty;

                    List<Dictionary<string, string>> modelValuesDictionary = (List<Dictionary<string, string>>)models[0].Resolution["values"];
                    var modelResult = modelValuesDictionary.First();
                    
                    result = DateTime.Parse(modelResult["value"]);
                    return true;
                }
            }
            catch (TooManyAttemptsException)
            {
                return false;
            }  
        }

        protected override async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> activity)
        {
            var message = await activity;
            var quitCondition = message.Text.ToLower().Contains("cancel");

            if (quitCondition)
            {
                context.Done(DateTime.MinValue);
            }
            else
            {
                var models = DateTimeRecognizer.RecognizeDateTime(message.Text, message.Locale);
                if (models == null || models.Count == 0)
                {
                    await context.PostAsync("Sorry, I didn't understand you, can you try again?", message.Locale);
                    context.Done(DateTime.MinValue);
                    return;
                }
                string data = string.Empty;

                List<Dictionary<string, string>> modelValuesDictionary = (List<Dictionary<string, string>>)models[0].Resolution["values"];
                var modelResult = modelValuesDictionary.First();
                if (modelResult["type"] == "time")
                {
                    var parsedTime = DateTime.Parse(modelResult["value"]);
                    timeValue = parsedTime.ToShortTimeString();

                    var response = shop.CanMakeAppointment(parsedTime);

                    if (response.IsValid)
                    {
                        var promptTodayConfirmation = new PromptDialog.PromptConfirm(
                            $"I understood {timeValue} but did you mean today?",
                            "Sorry I didn't understand you. Can you choose an option below?",
                            2);
                        context.Call(promptTodayConfirmation, this.AfterTodayConfirmation);
                    }
                    else
                    {
                        await context.PostAsync(response.FormattedErrorMessage(), message.Locale);
                        // todo, check and see if the appointment's barber is free earlier in the day 
                        // or suggest next available date and time.
                        context.Wait(this.MessageReceivedAsync);
                    }
                }
                else if (modelResult["type"] == "datetime")
                {
                    var parsedTime = DateTime.Parse(modelResult["value"]);
                    timeValue = parsedTime.ToShortTimeString();

                    var response = shop.CanMakeAppointment(parsedTime);
                    
                    if (response.IsValid)
                    {
                        context.Done(parsedTime);
                    }
                    else
                    {
                        await context.PostAsync(response.FormattedErrorMessage(), message.Locale);
                        // todo, check and see if the appointment's barber is free earlier in the day 
                        // or suggest next available date and time.
                        context.Wait(this.MessageReceivedAsync);
                    }
                }

            }

            //await base.MessageReceivedAsync(context, activity);
        }

        private async Task AfterTodayConfirmation(IDialogContext context, IAwaitable<bool> result)
        {
            bool today = await result;
            if (today)
            {
                DateTime value = DateTime.Parse(timeValue);
                await context.PostAsync("Ok", context.Activity.AsMessageActivity().Locale);
                context.Done(value);
            }
            else
            {
                await context.PostAsync("Ok, let's start again and you can tell me the day or date as well as the time. Or you can start over by with 'cancel'.", context.Activity.AsMessageActivity().Locale);
                context.Wait(this.MessageReceivedAsync);
                await context.PostAsync("Thanks.", context.Activity.AsMessageActivity().Locale);
            }
        }
    }

}