using System;
using Microsoft.Recognizers.Text.DateTime;
using System.Threading.Tasks;
using Xunit;
using Moq;

namespace BarberBot.Tests
{
    public class HoursTests
    {
        [Theory]
        [InlineData(DayOfWeek.Sunday, 0, 0)]
        [InlineData(DayOfWeek.Monday, 8, 20)]
        [InlineData(DayOfWeek.Tuesday, 8, 20)]
        [InlineData(DayOfWeek.Wednesday, 0, 0)]
        [InlineData(DayOfWeek.Thursday, 8, 20)]
        [InlineData(DayOfWeek.Friday, 8, 20)]
        [InlineData(DayOfWeek.Saturday, 8, 20)]
        public async Task BarberHours_LoadAsync_DaysOfWeekExpectedHours(DayOfWeek dayOfWeek, int expectedOpeningHour, int expectedClosingHour)
        {
            IHoursRepository repository = new MemoryHoursRepository();
            BarberHours barberHours = new BarberHours(repository);
            var expectedDateTime = DateTime.Now.This(dayOfWeek);
            Mock<IBarber> barberMock = new Mock<IBarber>();
            barberMock.Setup(s => s.Type).Returns(HoursType.Barber);
            await barberHours.LoadAsync(barberMock.Object, expectedDateTime);

            Assert.Equal(expectedOpeningHour, barberHours.OpeningHour);
            Assert.Equal(expectedClosingHour, barberHours.ClosingHour);
            Assert.Equal(expectedDateTime.Date, barberHours.OpeningDateTime().Date);
            Assert.Equal(expectedDateTime.Date, barberHours.ClosingDateTime().Date);
        }

        [Theory]
        [InlineData(DayOfWeek.Sunday, 8, 18)]
        [InlineData(DayOfWeek.Monday, 8, 20)]
        [InlineData(DayOfWeek.Tuesday, 8, 20)]
        [InlineData(DayOfWeek.Wednesday, 8, 20)]
        [InlineData(DayOfWeek.Thursday, 8, 20)]
        [InlineData(DayOfWeek.Friday, 8, 20)]
        [InlineData(DayOfWeek.Saturday, 8, 20)]
        public async Task ShopHours_LoadAsync_DaysOfWeekExpectedHours(DayOfWeek dayOfWeek, int expectedOpeningHour, int expectedClosingHour)
        {
            IHoursRepository repository = new MemoryHoursRepository();
            ShopHours shopHours = new ShopHours(repository);
            var expectedDateTime = DateTime.Now.This(dayOfWeek);
            Mock<IShop> shopMock = new Mock<IShop>();
            shopMock.Setup(s => s.Type).Returns(HoursType.Shop);
            await shopHours.LoadAsync(shopMock.Object, expectedDateTime);

            Assert.Equal(expectedOpeningHour, shopHours.OpeningHour);
            Assert.Equal(expectedClosingHour, shopHours.ClosingHour);
            Assert.Equal(expectedDateTime.Date, shopHours.OpeningDateTime().Date);
            Assert.Equal(expectedDateTime.Date, shopHours.ClosingDateTime().Date);
        }

        [Theory]
        [InlineData(DayOfWeek.Sunday, 19, false)]
        [InlineData(DayOfWeek.Monday, 8, true)]
        [InlineData(DayOfWeek.Tuesday, 9, true)]
        [InlineData(DayOfWeek.Wednesday, 10, true)]
        [InlineData(DayOfWeek.Thursday, 11, true)]
        [InlineData(DayOfWeek.Friday, 12, true)]
        [InlineData(DayOfWeek.Saturday, 13, true)]
        public async Task ShopHours_IsAvailableAsync_DaysOfWeekExpected(DayOfWeek dayOfWeek, int hour, bool expectedAvailability)
        {
            IHoursRepository repository = new MemoryHoursRepository();
            ShopHours shopHours = new ShopHours(repository);
            var date = DateTime.Now.This(dayOfWeek).Date;
            var inputDateTime = new DateTime(date.Year, date.Month, date.Day, hour, 0, 0);
            Mock<IShop> shopMock = new Mock<IShop>();
            shopMock.Setup(s => s.Type).Returns(HoursType.Shop);
            bool actualAvailability = await shopHours.IsAvailableAsync(shopMock.Object, inputDateTime);
            
            Assert.Equal(expectedAvailability, actualAvailability);
        }

        [Theory]
        [InlineData(DayOfWeek.Sunday, 19, false)]
        [InlineData(DayOfWeek.Monday, 8, true)]
        [InlineData(DayOfWeek.Tuesday, 10, true)]
        [InlineData(DayOfWeek.Wednesday, 10, false)]
        [InlineData(DayOfWeek.Thursday, 11, true)]
        [InlineData(DayOfWeek.Friday, 12, true)]
        [InlineData(DayOfWeek.Saturday, 13, true)]
        public async Task BarberHours_IsAvailableAsync_DaysOfWeekExpected(DayOfWeek dayOfWeek, int hour, bool expectedAvailability)
        {
            IHoursRepository repository = new MemoryHoursRepository();
            BarberHours barberHours = new BarberHours(repository);
            var date = DateTime.Now.This(dayOfWeek).Date;
            var inputDateTime = new DateTime(date.Year, date.Month, date.Day, hour, 0, 0);
            Mock<IBarber> barberMock = new Mock<IBarber>();
            barberMock.Setup(s => s.Type).Returns(HoursType.Barber);
            bool actualAvailability = await barberHours.IsAvailableAsync(barberMock.Object, inputDateTime);

            Assert.Equal(expectedAvailability, actualAvailability);
        }
    }
}
