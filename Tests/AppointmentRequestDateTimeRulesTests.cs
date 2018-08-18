using System;
using Microsoft.Recognizers.Text.DateTime;
using System.Threading.Tasks;
using Xunit;
using Moq;

namespace BarberBot.Tests
{
    public class AppointmentRequestDateTimeRulesTests
    {
        [Theory]
        [InlineData(0, 0, 0, 0)]
        [InlineData(0, 1, 0, 0)]
        [InlineData(0, 6, 0, 10)]
        [InlineData(0, 10, 0, 10)]
        [InlineData(0, 11, 0, 10)]
        [InlineData(0, 13, 0, 10)]
        [InlineData(0, 14, 0, 15)]
        [InlineData(0, 15, 0, 15)]
        [InlineData(0, 16, 0, 15)]
        [InlineData(0, 18, 0, 20)]
        [InlineData(0, 20, 0, 20)]
        [InlineData(0, 21, 0, 20)]
        [InlineData(0, 25, 0, 20)]
        [InlineData(0, 26, 0, 30)]
        [InlineData(0, 30, 0, 30)]
        [InlineData(0, 31, 0, 30)]
        [InlineData(0, 35, 0, 30)]
        [InlineData(0, 36, 0, 40)]
        [InlineData(0, 40, 0, 40)]
        [InlineData(0, 43, 0, 40)]
        [InlineData(0, 45, 0, 45)]
        [InlineData(0, 46, 0, 45)]
        [InlineData(0, 48, 0, 50)]
        [InlineData(0, 50, 0, 50)]
        [InlineData(0, 53, 0, 50)]
        [InlineData(0, 55, 0, 50)]
        [InlineData(0, 56, 1, 0)]
        public async Task BarberHours_LoadAsync_DaysOfWeekExpectedHours(int hour, int minute, int expectedHour, int expectedMinute)
        {
            AppointmentRequestDateTimeRounding rules = new AppointmentRequestDateTimeRounding();
            var now = DateTime.Now;
            DateTime result = await rules.RoundDateTimeAsync(new DateTime(now.Year, now.Month, now.Day, hour, minute, 0, DateTimeKind.Local));

            Assert.Equal(expectedHour, result.Hour);
            Assert.Equal(expectedMinute, result.Minute);
        }

    }
}
