using System;

namespace BarberBot
{
    [Serializable]
    public class BarberService
    {
        public string DisplayName { get; set; }
        public TimeSpan Duration { get; set; }
        public string Description { get; set; }

        public string ToDescriptionString()
        {
            return $"{ToString()}{Environment.NewLine}{Description}";
        }

        public override string ToString()
        {
            return $"{DisplayName} ({Duration.TotalMinutes} min)";
        }

        public override bool Equals(object obj)
        {
            if (obj.GetType() != this.GetType())
            {
                return false;
            }

            BarberService other = (BarberService)obj;
            return string.Equals(other.DisplayName, DisplayName, StringComparison.OrdinalIgnoreCase);
        }

        public override int GetHashCode()
        {
            return DisplayName.GetHashCode() ^ Duration.GetHashCode();
        }
    }
}