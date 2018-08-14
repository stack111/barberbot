namespace BarberBot
{
    public interface ISchedulable
    {
        HoursType Type { get; }
        void LoadFrom(Hours<ISchedulable> hours);
    }
}