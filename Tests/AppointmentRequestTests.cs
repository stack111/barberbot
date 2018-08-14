using System;
using System.Threading.Tasks;
using Xunit;

namespace BarberBot.Tests
{
    public class AppointmentRequestTests
    {

        [Fact]
        public async Task AppointmentRequest_IsAvaiableNowJessica_True()
        {
            var hoursRepository = new MemoryHoursRepository();
            var appointmentRepository = new MemoryAppointmentRepository();
            var barbersRepository = new MemoryBarbersRepository(appointmentRepository, hoursRepository);
            var storeHours = new ShopHours(hoursRepository);
            var barberHours = new BarberHours(hoursRepository);
            Shop shop = new Shop(appointmentRepository, storeHours, barbersRepository);
            AppointmentRequest request = new AppointmentRequest(shop);
            Barber initialBarber = new Barber(shop, appointmentRepository, barberHours);

            request.StartDateTime = DateTime.Now;
            initialBarber.DisplayName = "Jessica";
            request.RequestedBarber = initialBarber;
            request.Service = new BarberService() { DisplayName = "Short Hair Cut", Duration = TimeSpan.FromMinutes(30) };

            AppointmentAvailabilityResponse response = await request.IsAvailableAsync();

            Assert.True(response.IsAvailable, $"{initialBarber.DisplayName} should be availabe on {request.StartDateTime}");
        }
    }
}
