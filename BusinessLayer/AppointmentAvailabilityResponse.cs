﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace BarberBot
{
    public class AppointmentAvailabilityResponse
    {
        public AppointmentAvailabilityResponse()
        {
            ValidationResults = new List<ValidationResult>();
        }

        public bool IsAvailable { get; set; }
        public AppointmentRequest SuggestedRequest { get; set; }

        public List<ValidationResult> ValidationResults { get; set; }

        public string FormattedValidationMessage()
        {
            StringBuilder builder = new StringBuilder();
            foreach (var msg in ValidationResults)
            {
                builder.Append(msg.Message);
            }
            if(SuggestedRequest != null)
            {
                builder.AppendLine($"Instead would you like to reserve {SuggestedRequest.ToSuggestionString()}?");
            }
            return builder.ToString();
        }
    }
}