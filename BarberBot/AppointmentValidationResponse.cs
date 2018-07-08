using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace BarberBot
{
    public class AppointmentValidationResponse
    {
        public AppointmentValidationResponse()
        {
            ValidationResults = new List<ValidationResult>();
        }
        public bool IsValid { get; set; }
        public List<ValidationResult> ValidationResults { get; set; }

        public string FormattedErrorMessage()
        {
            StringBuilder builder = new StringBuilder("Sorry, ");
            foreach (var msg in ValidationResults)
            {
                builder.Append(msg.Message);
            }
            return builder.ToString();
        }
    }
}