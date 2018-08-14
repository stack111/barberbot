using System;

namespace BarberBot
{
    [Serializable]
    public class BarberHours : Hours<IBarber>
    {
        public BarberHours(IHoursRepository hoursRepository) : base(hoursRepository)
        {
        }

        public BarberHours(Hours<IBarber> hours) : base(hours)
        {
        }

        protected override string NotAvailableHoursString { get { return "N/A"; } }
    }
}