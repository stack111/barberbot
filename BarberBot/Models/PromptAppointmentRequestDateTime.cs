﻿namespace BarberBot.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Builder.Dialogs.Internals;
    using Microsoft.Bot.Connector;
    using Microsoft.Recognizers.Text.DateTime;

    [Serializable]
    public class PromptAppointmentRequestDateTime : Prompt<AppointmentRequest, string>
    {
        readonly DateTime PSTDate = DateTime.UtcNow.AddHours(-7).Date;
        readonly DateTime PSTDateTime = DateTime.UtcNow.AddHours(-7);
        readonly AppointmentRequest request;
        readonly int MaxNumberOfSuggestionAttempts = 3;
        string timeValue;
        private int suggestionAttemptCounter;
        AppointmentRequest suggestedAppointmentRequest;
        
        public PromptAppointmentRequestDateTime(AppointmentRequest request, string prompt, string retry = null, string tooManyAttempts = null, int attempts = 3)
          : base(new PromptOptions<string>(prompt, retry, tooManyAttempts, attempts: attempts))
        {
            this.request = request;
            suggestionAttemptCounter = 0;
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

                if (quitCondition)
                {
                    request.ResetDateTime();
                    context.Done(request);
                }
                else
                {
                    var recognizer = new DateTimeRecognizer(message.Locale, DateTimeOptions.None, false);
                    var dateTimeModel = recognizer.GetDateTimeModel();
                    var results = dateTimeModel.Parse(message.Text, PSTDateTime);

                    if (!results.Any())
                    {
                        request.ResetDateTime();
                        await context.PostAsync("Sorry, I didn't understand you, can you try again with a date and time?", message.Locale);
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
                                $"I assumed today and found {timeValue}, everything looks available at the moment but can confirm you that's what you meant?",
                                "Sorry I didn't understand you. Can you choose an option below?",
                                2);
                            context.Call(promptTodayConfirmation, this.AfterTodayConfirmation);
                        }
                        else
                        {
                            // we will send the suggestion so if the user confirms we can use it.
                            suggestedAppointmentRequest = response.SuggestedRequest;
                            var promptSuggestionConfirmation = new PromptDialog.PromptConfirm(
                               response.FormattedErrorMessage(),
                               "Sorry I didn't understand you. Can you choose an option below?",
                               2);
                            context.Call(promptSuggestionConfirmation, this.AfterSuggestionConfirmation);
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
                            await context.PostAsync("Great, that will work.", message.Locale);
                            context.Done(request);
                        }
                        else
                        {
                            suggestedAppointmentRequest = response.SuggestedRequest;
                            var promptSuggestionConfirmation = new PromptDialog.PromptConfirm(
                               response.FormattedErrorMessage(),
                               "Sorry I didn't understand you. Can you choose an option below?",
                               2);
                            context.Call(promptSuggestionConfirmation, this.AfterSuggestionConfirmation);
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
                               $"I found around {request.RequestedDateTime.ToShortTimeString()}, everything looks available at the moment but can you confirm that's what you meant?",
                               "Sorry I didn't understand you. Can you choose an option below?",
                               2);
                            context.Call(promptTodayConfirmation, this.AfterTimeConfirmation);
                        }
                        else
                        {
                            await context.PostAsync($"I looked around {request.RequestedDateTime.ToShortTimeString()}", context.Activity.AsMessageActivity().Locale);
                            suggestedAppointmentRequest = response.SuggestedRequest;
                            var promptSuggestionConfirmation = new PromptDialog.PromptConfirm(
                               response.FormattedErrorMessage().Replace("Sorry", "But sorry"),
                               "Sorry I didn't understand you. Can you choose an option below?",
                               2);
                            context.Call(promptSuggestionConfirmation, this.AfterSuggestionConfirmation);
                        }
                    }
                    else
                    {
                        request.ResetDateTime();
                        context.Done(request);
                    }
                }
            }
            catch (TooManyAttemptsException)
            {
                request.ResetDateTime();
                context.Done(request);
            }
        }

        private async Task AfterTimeConfirmation(IDialogContext context, IAwaitable<bool> result)
        {
            bool confirmed = await result;
            if (confirmed)
            {
                await context.PostAsync("Ok", context.Activity.AsMessageActivity().Locale);
                suggestionAttemptCounter = 0;
                context.Done(request);
            }
            else
            {
                await context.PostAsync("Ok, let's try again and you can tell me the date and time. Or you can exit with 'cancel'.", context.Activity.AsMessageActivity().Locale);
                DecideToContinueSuggestions(context, request);
            }
        }

        private async Task AfterSuggestionConfirmation(IDialogContext context, IAwaitable<bool> result)
        {
            bool confirmation = await result;
            if (confirmation)
            {
                await context.PostAsync("Great, we'll use that appointment.", context.Activity.AsMessageActivity().Locale);
                request.CopyFrom(suggestedAppointmentRequest);
                suggestionAttemptCounter = 0;
                context.Done(request);
            }
            else
            {
                request.ResetDateTime();
                await context.PostAsync("Ok, can you suggest another day and time which works for you? Or you can exit with 'cancel'.", context.Activity.AsMessageActivity().Locale);

                DecideToContinueSuggestions(context, request);
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
                suggestionAttemptCounter = 0;
                context.Done(request);
            }
            else
            {
                await context.PostAsync("Ok, let's try again and you can tell me the date and time. Or you can exit with 'cancel'.", context.Activity.AsMessageActivity().Locale);
                suggestionAttemptCounter++;
                DecideToContinueSuggestions(context, request);
            }
        }

        private void DecideToContinueSuggestions(IDialogContext context, AppointmentRequest request)
        {
            if (suggestionAttemptCounter < MaxNumberOfSuggestionAttempts)
            {
                suggestionAttemptCounter++;
                context.Wait(MessageReceivedAsync);
            }
            else
            {
                suggestionAttemptCounter = 0;
                request.ResetDateTime();
                context.Done(request);
            }
        }
    }

}