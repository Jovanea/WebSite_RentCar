using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Web.Models;
using System.Data.Entity;
using System.Data.SqlClient;
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
        public ActionResult Process(CardDetails cardDetails)
        {
            if (Session["Id"] == null)
            {
                return RedirectToAction("Login", "Home");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var cart = Session["Cart"] as List<Booking>;
                    if (cart == null || cart.Count == 0)
                    {
                        return RedirectToAction("Carsection", "Home");
                    }

                    // Calculează suma totală a comenzii
                    decimal totalAmount = cart.Sum(b => b.TotalAmount);

                    // Simulează procesarea plății
                    bool paymentSuccessful = ProcessPayment(cardDetails, totalAmount);

                    if (paymentSuccessful)
                    {
                        using (var db = new ApplicationDbContext())
                        {
                            // Pentru fiecare mașină din coș
                            foreach (var booking in cart)
                            {
                                // Actualizează booking-ul
                                booking.Status = "Confirmed";

                                // Dacă booking-ul nu există deja în baza de date, adăugă-l
                                var existingBooking = db.Bookings.Find(booking.BookingId);
                                if (existingBooking == null)
                                {
                                    // Generează un ID nou pentru booking-urile create în sesiune
                                   
                                        booking.BookingId = 0; // Permite bazei de date să genereze ID-ul
                                   
                                }
                                else
                                {
                                    // Actualizează booking-ul existent
                                    existingBooking.Status = "Confirmed";
                                    db.Entry(existingBooking).State = EntityState.Modified;
                                }

                                // Creează o înregistrare de plată
                                var payment = new Payment
                                {
                                    BookingId = 1,
                                    Amount = 300,
                                    PaymentDate = DateTime.Now,
                                    PaymentStatus = "Completed",
                                    TransactionId = Guid.NewGuid().ToString()
                                };

                                db.Payments.Add(payment);
                            }

                            // Salvează toate modificările în baza de date
                            db.SaveChanges();

                            // Salvează informațiile pentru pagina de succes
                            string transactionId = Guid.NewGuid().ToString();
                            Session["TransactionId"] = transactionId;
                            Session["PaymentAmount"] = totalAmount;
                            Session["PaymentDate"] = DateTime.Now;
                            Session["PaymentDetails"] = cart.Select(b => new
                            {
                                CarId = b.CarId,
                                PickupDate = b.PickupDate,
                                ReturnDate = b.ReturnDate,
                                TotalAmount = b.TotalAmount
                            }).ToList();

                            // Golește coșul
                            Session.Remove("Cart");
                            Session.Remove("CurrentBooking");

                            // Redirecționează către pagina de succes
                            return RedirectToAction("Index");
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("", "Procesarea plății a eșuat. Vă rugăm să verificați detaliile cardului și să încercați din nou.");
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Eroare la procesarea plății: {ex.Message}");
                    ModelState.AddModelError("", "A apărut o eroare în timpul procesării plății. Vă rugăm să încercați mai târziu.");
                }
            }

            // În caz de eroare, reafișează formularul cu datele introduse
            var cartForError = Session["Cart"] as List<Web.Models.Booking>;
            ViewBag.TotalAmount = cartForError != null ? cartForError.Sum(b => b.TotalAmount) : 0;

            return View(cardDetails);
        }

        private bool ProcessPayment(CardDetails cardDetails, decimal totalAmount)
        {
            return true;
        }

        // GET: Payment/Success
        public ActionResult Success()
        {
            if (Session["Id"] == null)
            {
                return RedirectToAction("Login", "Home");
            }

            // Transmitem informațiile despre plată către pagina de succes
            ViewBag.TransactionId = Session["TransactionId"];
            ViewBag.PaymentAmount = Session["PaymentAmount"];
            ViewBag.PaymentDate = Session["PaymentDate"];
            ViewBag.PaymentDetails = Session["PaymentDetails"];

            return View();
        }

        private bool ProcessPaymentWithGateway(CardDetails cardDetails, decimal amount)
        {
            // Simulează o procesare de plată
            // În aplicații reale, aici ar trebui să integrezi un serviciu de plăți real
            System.Threading.Thread.Sleep(1000);

            // Validare simplificată a cardului (doar pentru demonstrație)
            if (string.IsNullOrEmpty(cardDetails.CardNumber) ||
                string.IsNullOrEmpty(cardDetails.CardHolderName) ||
                string.IsNullOrEmpty(cardDetails.CVV) ||
                string.IsNullOrEmpty(cardDetails.ExpiryMonth) ||
                string.IsNullOrEmpty(cardDetails.ExpiryYear))
            {
                return false;
            }

            // Returnează succes pentru demonstrație
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