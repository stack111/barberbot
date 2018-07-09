using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BarberBot
{
    [Serializable]
    public class Appointment
    {
        private Shop shop;
        private Barber barber;
        private DateTime dateTime;

        public void CopyFrom(AppointmentRequest request)
        {
            shop = request.Shop;
            barber = request.RequestedBarber;
            dateTime = request.RequestedDateTime;
        }


    }
}