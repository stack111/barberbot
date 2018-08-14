using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BarberBot.Tests
{
    [TestClass]
    public class AppointmentRequestTests
    {
        AppointmentRequest request;
        Barber initialBarber;

        [TestInitialize]
        public void Init()
        {
            var hoursRepository = new MemoryHoursRepository();
            var appointmentRepository = new MemoryAppointmentRepository();
            var barbersRepository = new MemoryBarbersRepository(appointmentRepository, hoursRepository);
            var storeHours = new ShopHours(hoursRepository);
            var barberHours = new BarberHours(hoursRepository);
            Shop shop = new Shop(appointmentRepository, storeHours, barbersRepository);
            request = new AppointmentRequest(shop);
            initialBarber = new Barber(shop, appointmentRepository, barberHours);
        }

        [TestMethod]
        [TestCategory("L1")]
        public async Task AppointmentRequest_IsAvaiableNowJessica_True()
        {
            request.StartDateTime = DateTime.Now;
            initialBarber.DisplayName = "Jessica";
            request.RequestedBarber = initialBarber;
            request.Service = new BarberService() { DisplayName = "Short Hair Cut", Duration = TimeSpan.FromMinutes(30) };

            AppointmentAvailabilityResponse response = await request.IsAvailableAsync();

            Assert.IsTrue(response.IsAvailable, $"{initialBarber.DisplayName} should be availabe on {request.StartDateTime}");
        }
    }
}
