using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Web.Models;
using System.Data.Entity;
using eUseControl.Domain.Entities.Car;

namespace Web.Controllers
{
    public class PaymentController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Payment/Process
        public ActionResult Process(int? bookingId)
        {
            if (Session["UserID"] == null)
            {
                return RedirectToAction("Login", "Home");
            }

            // If bookingId is not provided, try to get it from session
            if (!bookingId.HasValue)
            {
                var booking = Session["CurrentBooking"] as Booking;
                if (booking != null)
                {
                    bookingId = booking.BookingId;
                }
                else
                {
                    return RedirectToAction("Carsection", "Home");
                }
            }

            var bookingFromDb = db.Bookings.Find(bookingId);
            if (bookingFromDb == null)
            {
                return HttpNotFound();
            }

            // Verify that the booking belongs to the current user
            if (bookingFromDb.UserId != (int)Session["UserID"])
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.Forbidden);
            }

            ViewBag.BookingId = bookingId;
            ViewBag.Amount = bookingFromDb.TotalAmount;
            return View(new CardDetails());
        }

        // POST: Payment/Process
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Process(CardDetails cardDetails, int bookingId)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var booking = db.Bookings.Find(bookingId);
                    if (booking == null)
                    {
                        return HttpNotFound();
                    }

                    // Verify that the booking belongs to the current user
                    if (booking.UserId != (int)Session["UserID"])
                    {
                        return new HttpStatusCodeResult(System.Net.HttpStatusCode.Forbidden);
                    }

                    // Process payment with payment gateway
                    bool paymentSuccessful = ProcessPaymentWithGateway(cardDetails, booking.TotalAmount);


                    if (paymentSuccessful)
                    {
                        // Create payment record
                        var payment = new Payment
                        {
                            BookingId = bookingId,
                            Amount = booking.TotalAmount,
                            PaymentDate = DateTime.Now,
                            PaymentStatus = "Completed",
                            TransactionId = Guid.NewGuid().ToString()
                        };

                        db.Payments.Add(payment);
                        booking.Status = "Paid";
                        db.Entry(booking).State = EntityState.Modified;

                        db.SaveChanges();

                        // Clear the booking from session
                        Session.Remove("CurrentBooking");


                        return RedirectToAction("Success", new { id = payment.PaymentId });
                    }
                    else
                    {
                        ModelState.AddModelError("", "Procesarea plății a eșuat. Vă rugăm să verificați detaliile cardului și să încercați din nou.");
                    }
                }
                catch (Exception ex)
                {
                    // Înregistrare eroare în jurnal
                    System.Diagnostics.Debug.WriteLine($"Eroare la procesarea plății: {ex.Message}");
                    ModelState.AddModelError("", "A apărut o eroare în timpul procesării plății. Vă rugăm să încercați mai târziu.");
                }

            }

            ViewBag.BookingId = bookingId;
            ViewBag.Amount = db.Bookings.Find(bookingId).TotalAmount;
            return View(cardDetails);
        }

        // GET: Payment/Success
        public ActionResult Success(int id)
        {
            if (Session["UserID"] == null)
            {
                return RedirectToAction("Login", "Home");
            }

            var payment = db.Payments.Find(id);
            if (payment == null)
            {
                return HttpNotFound();
            }

            // Verify that the payment belongs to the current user
            var booking = db.Bookings.Find(payment.BookingId);
            if (booking == null || booking.UserId != (int)Session["UserID"])
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.Forbidden);
            }

            return View(payment);
        }

        private bool ProcessPaymentWithGateway(CardDetails cardDetails, decimal amount)
        {
            System.Threading.Thread.Sleep(1000);

            return true;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}