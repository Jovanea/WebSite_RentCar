using System;
using System.Collections.Generic;
using System.Linq;
using Web.Interfaces;
using eUseControl.BusinessLogic.DBModel;

namespace Web.BusinessLogic
{
    public class BookingApi : IBookingApi
    {
        private readonly ApplicationDbContext _context;

        public BookingApi()
        {
            _context = new ApplicationDbContext();
        }

        public bool CreateBooking(Booking booking)
        {
            try
            {
                _context.Bookings.Add(booking);
                _context.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool UpdateBooking(Booking booking)
        {
            try
            {
                var existingBooking = _context.Bookings.Find(booking.BookingId);
                if (existingBooking == null) return false;

                _context.Entry(existingBooking).CurrentValues.SetValues(booking);
                _context.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool CancelBooking(int bookingId)
        {
            try
            {
                var booking = _context.Bookings.Find(bookingId);
                if (booking == null) return false;

                booking.Status = "Cancelled";
                _context.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public List<Booking> GetUserBookings(int userId)
        {
            return _context.Bookings.Where(b => b.UserId == userId).ToList();
        }

        public Booking GetBookingById(int bookingId)
        {
            return _context.Bookings.Find(bookingId);
        }
    }
}
