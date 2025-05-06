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

        public ActionResult Process()
        {
            if (Session["Id"] == null)
            {
                return RedirectToAction("Login", "Home");
            }

            var cart = Session["Cart"] as List<Booking>;
            if (cart == null || cart.Count == 0)
            {
                return RedirectToAction("Carsection", "Home");
            }

            decimal totalAmount = cart.Sum(b => b.TotalAmount);


            ViewBag.Amount = totalAmount;
            System.Diagnostics.Debug.WriteLine($"DEBUG: Total Amount={totalAmount} ({totalAmount.GetType()})");

            var model = new CardDetails();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Process(CardDetails cardDetails)
        {
            if (Session["Id"] == null)
            {
                return RedirectToAction("Login", "Home");
            }

            var cart = Session["Cart"] as List<Booking>;
            if (cart == null || cart.Count == 0)
            {
                return RedirectToAction("Carsection", "Home");
            }

            decimal totalAmount = cart.Sum(b => b.TotalAmount);

            if (!ModelState.IsValid)
            {
                ViewBag.Amount = totalAmount;
                return View(cardDetails);
            }


            if (!cardDetails.IsNumberValid())
            {
                ModelState.AddModelError("", "Numărul cardului este invalid. Vă rugăm să introduceți un număr valid de 16 cifre.");
                ViewBag.Amount = totalAmount;
                return View(cardDetails);
            }

            if (!cardDetails.IsExpiryDateValid())
            {
                ModelState.AddModelError("", "Data de expirare a cardului este invalidă sau a expirat.");
                ViewBag.Amount = totalAmount;
                return View(cardDetails);
            }

            if (cardDetails.CardNumber.Length != 19)
            {

                ModelState.AddModelError("", "Numărul cardului trebuie să conțină exact 16 cifre.");
                ViewBag.Amount = totalAmount;
                return View(cardDetails);
            }

            if (verificNume(cardDetails.CardHolderName) == false)
            {

                ModelState.AddModelError("", "Numele cardului este invalid");
                ViewBag.Amount = totalAmount;
                return View(cardDetails);
            }

            if (cardDetails.CVV.Length != 3)
            {

                ModelState.AddModelError("", "CVV-ul trebuie să conțină exact 3 cifre.");
                ViewBag.Amount = totalAmount;
                return View(cardDetails);
            }
            
            bool paymentSuccessful = ProcessPayment(cardDetails, totalAmount);

            if (paymentSuccessful)
            {
                using (var transaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        foreach (var booking in cart)
                        {
                        
                            booking.Status = "Confirmed";
                            db.Entry(booking).State = EntityState.Modified;

                        }

                        transaction.Commit();

                        Session["TransactionId"] = Guid.NewGuid().ToString();
                        Session["PaymentAmount"] = totalAmount;
                        Session["PaymentDate"] = DateTime.Now;
                        Session["PaymentDetails"] = cart.Select(b => new
                        {
                            CarId = b.CarId,
                            PickupDate = b.PickupDate,
                            ReturnDate = b.ReturnDate,
                            TotalAmount = b.TotalAmount
                        }).ToList();

                        Session.Remove("Cart");
                        Session.Remove("CurrentBooking");

                        return RedirectToAction("Success");
                    }
                    catch
                    {
                        transaction.Rollback();
                        ViewBag.Amount = totalAmount;
                        ModelState.AddModelError("", "A apărut o eroare în timpul procesării plății. Vă rugăm să încercați din nou.");
                        return View(cardDetails);
                    }
                }
            }
            else
            {
                ViewBag.Amount = totalAmount;
                ModelState.AddModelError("", "Procesarea plății a eșuat. Vă rugăm să verificați detaliile cardului și să încercați din nou.");
                return View(cardDetails);
            }
        }

        private bool verificNume(string card)
        {
            if (card[card.Length - 1] == 32)
                return false;
            int a = 0;
            for(int i = 0; i < card.Length; i++)
            {
                if (card[i] == 32 && a == 1)
                {
                    return false;
                } else if (card[i] == 32  && a == 0)
                {
                    a = 1;
                }
            }
            if (a == 0)
                return false;

            return true;
        }

        private bool ProcessPayment(CardDetails cardDetails, decimal amount)
        {
            try
            {
                if (amount <= 0)
                {
                    System.Diagnostics.Debug.WriteLine("Eroare: amount <= 0");
                    return false;
                }

                if (!cardDetails.IsExpiryDateValid())
                {
                    System.Diagnostics.Debug.WriteLine("Eroare: CardNumber invalid");
                    return false;
                }
                if (!cardDetails.IsExpiryDateValid())
                {
                    System.Diagnostics.Debug.WriteLine("Eroare: ExpiryDate invalid");
                    return false;
                }
                if (string.IsNullOrEmpty(cardDetails.CVV) || cardDetails.CVV.Length != 3 || !cardDetails.CVV.All(char.IsDigit))
                {
                    System.Diagnostics.Debug.WriteLine("Eroare: CVV invalid");
                    return false;
                }
                if (string.IsNullOrEmpty(cardDetails.CardHolderName) || !cardDetails.CardHolderName.All(c => char.IsLetter(c) || char.IsWhiteSpace(c)))
                {
                    System.Diagnostics.Debug.WriteLine("Eroare: CardHolderName invalid");
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Exceptie: " + ex.Message);
                return false;
            }
        }

        public ActionResult Success()
        {
            if (Session["Id"] == null)
            {
                return RedirectToAction("Login", "Home");
            }

            ViewBag.TransactionId = Session["TransactionId"];
            ViewBag.PaymentAmount = Session["PaymentAmount"];
            ViewBag.PaymentDate = Session["PaymentDate"];
            ViewBag.PaymentDetails = Session["PaymentDetails"];

            return View();
        }

        private bool ProcessPaymentWithGateway(CardDetails cardDetails, decimal amount)
        {
            System.Threading.Thread.Sleep(1000);

            if (string.IsNullOrEmpty(cardDetails.CardNumber) ||
                string.IsNullOrEmpty(cardDetails.CardHolderName) ||
                string.IsNullOrEmpty(cardDetails.CVV) ||
                string.IsNullOrEmpty(cardDetails.ExpiryMonth) ||
                string.IsNullOrEmpty(cardDetails.ExpiryYear))
            {
                return false;
            }

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