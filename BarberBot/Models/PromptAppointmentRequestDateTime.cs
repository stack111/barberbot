namespace BarberBot.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Builder.Dialogs.Internals;
    using Microsoft.Bot.Connector;
    using Microsoft.Recognizers.Text.Choice;
    using Microsoft.Recognizers.Text.DateTime;

    [Serializable]
    public class PromptAppointmentRequestDateTime : Prompt<AppointmentRequest, string>
    {
        readonly DateTime PSTDate = DateTime.UtcNow.AddHours(-7).Date;
        readonly DateTime PSTDateTime = DateTime.UtcNow.AddHours(-7);
        private readonly AppointmentRequest request;
        string timeValue;
        bool hasAttempted;
        AppointmentRequest suggestedAppointmentRequest;
        
        public PromptAppointmentRequestDateTime(AppointmentRequest request, string prompt, string retry = null, string tooManyAttempts = null, int attempts = 3)
          : base(new PromptOptions<string>(prompt, retry, tooManyAttempts, attempts: attempts))
        {
            this.request = request;
        }

        protected override bool TryParse(IMessageActivity message, out AppointmentRequest result)
        {
            result = null;
            try
            {
                var quitCondition = message.Text.ToLower().Contains("cancel");

                if (quitCondition)
                {
                    return quitCondition;
                }
                else
                {
                    // currently unused
                    result = null;
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
            try
            {
                var message = await activity;
                var quitCondition = message.Text.ToLower().Contains("cancel");

                if (hasAttempted)
                {
                    var choiceModels = ChoiceRecognizer.RecognizeBoolean(message.Text, message.Locale);
                    if (!choiceModels.Any())
                    {
                        request.ResetDateTime();
                        await context.PostAsync("Sorry, I didn't understand you, can you try again with the appointment date and time?", message.Locale);
                        hasAttempted = false;
                        context.Wait(MessageReceivedAsync);
                        return;
                    }
                    else
                    {
                        bool confirmation = (bool)choiceModels[0].Resolution["value"];
                        double score = (double)choiceModels[0].Resolution["score"];
                        if (score <= .5)
                        {
                            var promptSuggestionConfirmation = new PromptDialog.PromptConfirm(
                                $"Sorry, I didn't get that, can you confirm you are ok with the suggested appointment?",
                                "Sorry I didn't understand you. Can you choose an option below?",
                                2);
                            context.Call(promptSuggestionConfirmation, this.AfterSuggestionConfirmation);
                            return;
                        }
                        else if (confirmation)
                        {
                            await context.PostAsync("Great, we'll use that appointment.", message.Locale);
                            hasAttempted = false;
                            request.CopyFrom(suggestedAppointmentRequest);
                            context.Done(request);
                            return;
                        }
                        else
                        {
                            request.ResetDateTime();
                            await context.PostAsync("Ok, can you suggest another day and time which works for you?", message.Locale);
                            hasAttempted = false;
                            context.Wait(MessageReceivedAsync);
                            return;
                        }
                    }
                }

                if (quitCondition)
                {
                    request.ResetDateTime();
                    hasAttempted = false;
                    context.Done(request);
                }
                else
                {
                    var recognizer = new DateTimeRecognizer(message.Locale, DateTimeOptions.None, false);
                    var dateTimeModel = recognizer.GetDateTimeModel();
                    var results = dateTimeModel.Parse(message.Text, PSTDate);

                    if (!results.Any())
                    {
                        request.ResetDateTime();
                        await context.PostAsync("Sorry, I didn't understand you, can you try again?", message.Locale);
                        hasAttempted = false;
                        context.Wait(MessageReceivedAsync);
                        return;
                    }

                    List<Dictionary<string, string>> modelValuesDictionary = (List<Dictionary<string, string>>)results[0].Resolution["values"];
                    var modelResult = modelValuesDictionary.First();
                    if (modelResult["type"] == "time")
                    {
                        var parsedTime = DateTime.Parse(modelResult["value"]);
                        request.RequestedDateTime = parsedTime;
                        timeValue = parsedTime.ToShortTimeString();
                        var response = await request.IsAvailableAsync();

                        if (response.IsAvailable)
                        {
                            var promptTodayConfirmation = new PromptDialog.PromptConfirm(
                                $"I assumed today and found {timeValue}, everything looks available at the moment but I need you to confirm that's what you meant and not a different day.",
                                "Sorry I didn't understand you. Can you choose an option below?",
                                2);
                            context.Call(promptTodayConfirmation, this.AfterTodayConfirmation);
                        }
                        else
                        {
                            // we will send the suggestion so if the user confirms we can use it.
                            suggestedAppointmentRequest = response.SuggestedRequest;
                            await context.PostAsync(response.FormattedErrorMessage(), message.Locale);
                            hasAttempted = true;
                            context.Wait(this.MessageReceivedAsync);
                        }
                    }
                    else if (modelResult["type"] == "datetime")
                    {
                        var parsedTime = DateTime.Parse(modelResult["value"]);
                        timeValue = parsedTime.ToShortTimeString();
                        request.RequestedDateTime = parsedTime;
                        var response = await request.IsAvailableAsync();

                        if (response.IsAvailable)
                        {
                            hasAttempted = false;
                            await context.PostAsync("Great, that will work.", message.Locale);
                            context.Done(request);
                        }
                        else
                        {
                            suggestedAppointmentRequest = response.SuggestedRequest;
                            await context.PostAsync(response.FormattedErrorMessage(), message.Locale);
                            hasAttempted = true;
                            context.Wait(this.MessageReceivedAsync);
                        }
                    }
                    else if (modelResult["type"] == "date")
                    {
                        var parsedDate = DateTime.Parse(modelResult["value"]);
                        request.RequestedDateTime = new DateTime(parsedDate.Year, parsedDate.Month, parsedDate.Day, PSTDateTime.Hour, PSTDateTime.Minute, 0, DateTimeKind.Local);
                        var response = await request.IsAvailableAsync();
                        if (response.IsAvailable)
                        {
                            var promptTodayConfirmation = new PromptDialog.PromptConfirm(
                               $"I found around {request.RequestedDateTime.ToShortTimeString()}, everything looks available at the moment but I need you to confirm that's what you meant at this time not another time.",
                               "Sorry I didn't understand you. Can you choose an option below?",
                               2);
                            context.Call(promptTodayConfirmation, this.AfterTimeConfirmation);
                        }
                        else
                        {
                            await context.PostAsync($"I guessed around {request.RequestedDateTime.ToShortTimeString()}", context.Activity.AsMessageActivity().Locale);
                            suggestedAppointmentRequest = response.SuggestedRequest;
                            await context.PostAsync(response.FormattedErrorMessage().Replace("Sorry", "But sorry"), message.Locale);
                            hasAttempted = true;
                            context.Wait(this.MessageReceivedAsync);
                        }
                    }
                }
            }
            catch (TooManyAttemptsException ex)
            {
                request.ResetDateTime();
                context.Fail(ex);
            }
        }

        private async Task AfterTimeConfirmation(IDialogContext context, IAwaitable<bool> result)
        {
            bool confirmed = await result;
            if (confirmed)
            {
                await context.PostAsync("Ok", context.Activity.AsMessageActivity().Locale);
                hasAttempted = false;
                context.Done(request);
            }
            else
            {
                await context.PostAsync("Ok, let's try again and you can tell me the date and time. Or you can exit with 'cancel'.", context.Activity.AsMessageActivity().Locale);
                hasAttempted = false;
                context.Wait(MessageReceivedAsync);
            }
        }

        private async Task AfterSuggestionConfirmation(IDialogContext context, IAwaitable<bool> result)
        {
            bool confirmation = await result;
            if (confirmation)
            {
                await context.PostAsync("Great, we'll use that appointment.", context.Activity.AsMessageActivity().Locale);
                hasAttempted = false;
                request.CopyFrom(suggestedAppointmentRequest);
                context.Done(request);
            }
            else
            {
                request.ResetDateTime();
                await context.PostAsync("Ok, can you suggest another day and time which works for you?", context.Activity.AsMessageActivity().Locale);
                hasAttempted = false;
                context.Wait(MessageReceivedAsync);
                context.Done(request);
            }
        }

        private async Task AfterTodayConfirmation(IDialogContext context, IAwaitable<bool> result)
        {
            bool today = await result;
            if (today)
            {
                DateTime todayDate = PSTDate;
                DateTime timeInstance = DateTime.Parse(timeValue);
                todayDate = new DateTime(todayDate.Year, todayDate.Month, todayDate.Day, timeInstance.Hour, timeInstance.Minute, 0, DateTimeKind.Local);
                
                request.RequestedDateTime = todayDate;
                await context.PostAsync("Ok", context.Activity.AsMessageActivity().Locale);
                hasAttempted = false;
                context.Done(request);
            }
            else
            {
                await context.PostAsync("Ok, let's try again and you can tell me the date and time. Or you can exit with 'cancel'.", context.Activity.AsMessageActivity().Locale);
                hasAttempted = false;
                context.Wait(MessageReceivedAsync);
            }
        }
    }

}