using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace BarberBot.Tests
{
    public class AppointmentTests
    {
        [Theory]
        [InlineData(30, -30, false)]
        [InlineData(30, -10, true)]
        [InlineData(30, 30, false)]
        [InlineData(15, -15, false)]
        [InlineData(20, -15, true)]
        [InlineData(20, 5, true)]
        [InlineData(30, 10, true)]
        [InlineData(40, 0, true)]
        [InlineData(40, 4, true)]
        public void Appointment_IsConflicting(int serviceMinuteLength, int conflictingMinuteStartTimeAddition, bool expectedConflict)
        {
            Mock<IRepository<Appointment>> appointmentRepoMock = new Mock<IRepository<Appointment>>();
            Mock<IShop> shopMock = new Mock<IShop>();
            Mock<IBarber> barberMock = new Mock<IBarber>();
            barberMock.Setup(b => b.DisplayName)
                .Returns("Barber")
                .Verifiable();

            barberMock.Setup(b => b.Equals(It.Is<IBarber>(o => o.DisplayName == "Barber")))
                .Returns(true)
                .Verifiable();

            var now = DateTime.Now;
            AppointmentRequest initialRequest = new AppointmentRequest(shopMock.Object)
            {
                StartDateTime = now,
                RequestedBarber = barberMock.Object,
                Service = new BarberService() { DisplayName = "Some Service", Duration = TimeSpan.FromMinutes(30) }
            };
            Appointment appointment = new Appointment(appointmentRepoMock.Object);
            appointment.CopyFrom(initialRequest);

            Appointment conflictingAppt = new Appointment(appointmentRepoMock.Object);
            AppointmentRequest conflictingRequest = new AppointmentRequest(shopMock.Object)
            {
                StartDateTime = now.AddMinutes(conflictingMinuteStartTimeAddition),
                RequestedBarber = barberMock.Object,
                Service = new BarberService() { DisplayName = "Some Service", Duration = TimeSpan.FromMinutes(serviceMinuteLength) }
            };
            conflictingAppt.CopyFrom(conflictingRequest);

            bool result = appointment.IsConflicting(conflictingAppt);

            Assert.Equal(expectedConflict, result);
            barberMock.VerifyAll();
        }

        [Fact]
        public void Appointment_IsConflicting_730()
        {
            Mock<IRepository<Appointment>> appointmentRepoMock = new Mock<IRepository<Appointment>>();
            Mock<IShop> shopMock = new Mock<IShop>();
            Mock<IBarber> barberMock = new Mock<IBarber>();
            barberMock.Setup(b => b.DisplayName)
                .Returns("Barber")
                .Verifiable();

            barberMock.Setup(b => b.Equals(It.Is<IBarber>(o => o.DisplayName == "Barber")))
                .Returns(true)
                .Verifiable();

            var now = new DateTime(2018, 1, 1, 7, 30, 0);
            AppointmentRequest initialRequest = new AppointmentRequest(shopMock.Object)
            {
                StartDateTime = now,
                RequestedBarber = barberMock.Object,
                Service = new BarberService() { DisplayName = "Some Service", Duration = TimeSpan.FromMinutes(40) }
            };
            Appointment appointment = new Appointment(appointmentRepoMock.Object);
            appointment.CopyFrom(initialRequest);

            Appointment conflictingAppt = new Appointment(appointmentRepoMock.Object);
            AppointmentRequest conflictingRequest = new AppointmentRequest(shopMock.Object)
            {
                StartDateTime = now.AddMinutes(-15),
                RequestedBarber = barberMock.Object,
                Service = new BarberService() { DisplayName = "Some Service", Duration = TimeSpan.FromMinutes(40) }
            };
            conflictingAppt.CopyFrom(conflictingRequest);

            bool result = appointment.IsConflicting(conflictingAppt);

            Assert.True(result);
            barberMock.VerifyAll();
        }
    }
}
