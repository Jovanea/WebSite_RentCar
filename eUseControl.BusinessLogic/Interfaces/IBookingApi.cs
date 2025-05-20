using System;
using System.Collections.Generic;
using eUseControl.BusinessLogic.DBModel;

namespace Web.Interfaces
{
    public interface IBookingApi
    {
        bool CreateBooking(Booking booking);
        bool UpdateBooking(Booking booking);
        bool CancelBooking(int bookingId);
        List<Booking> GetUserBookings(int userId);
        Booking GetBookingById(int bookingId);
    }
}
