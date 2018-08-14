using System;

namespace BarberBot
{
    [Serializable]
    public class BarberHours : Hours<Barber>
    {
        public BarberHours(IHoursRepository hoursRepository) : base(hoursRepository)
        {
        }

        public BarberHours(Hours<Barber> hours) : base(hours)
        {
        }

        protected override string NotAvailableHoursString { get { return "N/A"; } }
    }
}