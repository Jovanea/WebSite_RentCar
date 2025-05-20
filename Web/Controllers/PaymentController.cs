using System;
using System.Web.Mvc;
using Web.Interfaces;
using Web.BusinessLogic;
using eUseControl.BusinessLogic.DBModel;
using System.Linq;

namespace Web.Controllers
{
    public class PaymentController : Controller
    {
        private readonly IPaymentApi _paymentApi;
        private readonly IBookingApi _bookingApi;

        public PaymentController()
        {
            _paymentApi = new PaymentApi();
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

            var booking = _bookingApi.GetUserBookings((int)Session["Id"])
                .FirstOrDefault(b => b.Status == "Pending");

            if (booking == null)
            {
                return RedirectToAction("Carsection", "Home");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Amount = booking.TotalAmount;
                return View(cardDetails);
            }

            if (_paymentApi.ProcessPayment(cardDetails, booking.TotalAmount))
            {
                booking.Status = "Paid";
                if (_bookingApi.UpdateBooking(booking))
                {
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
            }

            ViewBag.Amount = booking.TotalAmount;
            ModelState.AddModelError("", "Procesarea plății a eșuat. Vă rugăm să verificați detaliile cardului și să încercați din nou.");
            return View(cardDetails);
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

            return View();
        }
    }
}