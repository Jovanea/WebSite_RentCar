using System;
using System.Web.Mvc;
using Web.Interfaces;
using Web.BusinessLogic;
using eUseControl.BusinessLogic.DBModel;
using System.Linq;
using System.Collections.Generic;

namespace Web.Controllers
{
    public class PaymentController : Controller
    {
        private readonly IBookingApi _bookingApi;

        public PaymentController()
        {
            _bookingApi = new BookingApi();
        }

        public ActionResult Process()
        {
            if (Session["Id"] == null)
            {
                return RedirectToAction("Login", "Home");
            }

            var booking = _bookingApi.GetUserBookings((int)Session["Id"])
                .FirstOrDefault(b => b.Status == "Pending");

            if (booking == null)
            {
                return RedirectToAction("Carsection", "Home");
            }

            ViewBag.Amount = booking.TotalAmount;
            return View(new CardDetails());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Process(CardDetails cardDetails)
        {
            if (Session["Id"] == null)
            {
                return RedirectToAction("Login", "Home");
            }

            try
            {
                int userId = 0;
                if (Session["Id"] is int)
                {
                    userId = (int)Session["Id"];
                }
                else if (int.TryParse(Session["Id"].ToString(), out int parsedId))
                {
                    userId = parsedId;
                }

                var pendingBookings = _bookingApi.GetUserBookings(userId);
                var booking = pendingBookings.FirstOrDefault(b => b.Status == "Pending");

                if (booking == null)
                {
                    System.Diagnostics.Debug.WriteLine("No pending booking found, creating success transaction anyway for demo");

                    return RedirectToAction("Success");
                }

                cardDetails.BookingId = booking.BookingId;

                System.Diagnostics.Debug.WriteLine($"Processing payment for booking {booking.BookingId}");
                System.Diagnostics.Debug.WriteLine($"Card holder: {cardDetails.CardHolderName}");
                System.Diagnostics.Debug.WriteLine($"Card number length: {cardDetails.CardNumber?.Replace(" ", "").Length} digits");
                System.Diagnostics.Debug.WriteLine($"Expiry: {cardDetails.ExpiryMonth}/{cardDetails.ExpiryYear}");

                bool paymentResult = true;

                System.Diagnostics.Debug.WriteLine($"Payment result: {paymentResult}");

                booking.Status = "Paid";
                bool bookingUpdated = false;
                try
                {
                    bookingUpdated = _bookingApi.UpdateBooking(booking);

                }
                catch (Exception ex)
                {


                    TempData["BookingUpdateError"] = $"Eroare la actualizarea rezervării: {ex.Message}";
                }

                Session["TransactionId"] = Guid.NewGuid().ToString();
                Session["PaymentAmount"] = booking.TotalAmount;
                Session["PaymentDate"] = DateTime.Now;
                Session["PaymentDetails"] = new
                {
                    CarId = booking.CarId,
                    PickupDate = booking.PickupDate,
                    ReturnDate = booking.ReturnDate,
                    TotalAmount = booking.TotalAmount
                };

                return RedirectToAction("Success");
            }
            catch
            {



                return RedirectToAction("Success");
            }
        }

        public ActionResult Success()
        {
            if (Session["TransactionId"] == null)
            {
                return RedirectToAction("Index", "Home");
            }

            ViewBag.TransactionId = Session["TransactionId"];
            ViewBag.Amount = Session["PaymentAmount"];
            ViewBag.PaymentDate = Session["PaymentDate"];
            ViewBag.PaymentDetails = Session["PaymentDetails"];

            Session["Cart"] = new List<Booking>();

            Session["TransactionId"] = null;
            Session["PaymentAmount"] = null;
            Session["PaymentDate"] = null;
            Session["PaymentDetails"] = null;

            return View();
        }
    }
}