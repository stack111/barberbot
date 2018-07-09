using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BarberBot
{
    [Serializable]
    public class BarberAvailabilityResponse
    {
        public BarberAvailabilityResponse()
        {
            ValidationResults = new List<ValidationResult>();
        }

        public bool IsAvailable { get; set; }
        public List<ValidationResult> ValidationResults { get; set; }
        public Barber Barber { get; internal set; }
    }
}