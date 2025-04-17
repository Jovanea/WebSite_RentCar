using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using eUseControl.Domain.Entities.User;
using eUseControl.BusinessLogic;
using eUseControl.BusinessLogic.Interfaces;
using Web.Models;

namespace Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IUserApi _userApi;

        public HomeController()
        {
            _userApi = new UserApi();
        }

        // GET: Home
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(UserLogin login)
        {
            if (ModelState.IsValid)
            {
                ULoginData data = new ULoginData
                {
                    Credential = login.Credential,
                    Password = login.Password,
                    LoginIp = Request.UserHostAddress,
                    LoginDateTime = DateTime.Now
                };

                var userLogin = _userApi.UserLogin(data);

                if (userLogin.Status)
                {
                    // Store user information in session
                    Session["Id"] = login.Credential;

                    // Verifică dacă trebuie să redirecționeze către plată
                    if (Request.Form["redirectToPayment"] != null && Request.Form["redirectToPayment"].ToString() == "true")
                    {
                        return RedirectToAction("Pay", "Home");
                    }

                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError("", userLogin.StatusMsg);
                    return View("Login");
                }
            }
            return View("Login");
        }

        public ActionResult Services()
        {
            return View();
        }

        public ActionResult Blogs()
        {
            return View();
        }

        public ActionResult Carsection()
        {
            return View();
        }

        public ActionResult Cardetalies(int id = 1)
        {
            ViewBag.CarId = id;
            return View();
        }

        public ActionResult Login()
        {
            // Verifică dacă utilizatorul vine de la pagina de coș (pentru plată)
            if (TempData["RedirectToPayment"] != null && (bool)TempData["RedirectToPayment"])
            {
                ViewBag.RedirectToPayment = true;
            }
            return View();
        }

        public ActionResult about()
        {
            return View();
        }

        public ActionResult Contacts()
        {
            return View();
        }
        public ActionResult Registre()
        {
            return View();
        }

        public new ActionResult Profile()
        {
            return View();
        }

        public ActionResult Review()
        {
            return View();
        }

        public ActionResult Cos()
        {
            return View();
        }

        public ActionResult Pay()
        {
            // Check if user is logged in
            if (Session["Id"] == null)
            {
                // Setează flag pentru redirecționare după login
                TempData["RedirectToPayment"] = true;
                return RedirectToAction("Login", "Home");
            }

            // Get the booking from session or create a new one
            var cart = Session["Cart"] as List<Web.Models.Booking>;
            if (cart == null || cart.Count == 0)
            {
                return RedirectToAction("Carsection", "Home");
            }

            // Pregătește datele pentru plată
            ViewBag.TotalAmount = cart.Sum(b => b.TotalAmount);

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Pay(CardDetails cardDetails)
        {
            if (Session["Id"] == null)
            {
                return RedirectToAction("Login", "Home");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var cart = Session["Cart"] as List<Web.Models.Booking>;
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
                                    if (booking.BookingId < 1000)
                                    {
                                        booking.BookingId = 0; // Permite bazei de date să genereze ID-ul
                                    }
                                    db.Bookings.Add(booking);
                                   
                                }
                                else
                                {
                                    // Actualizează booking-ul existent
                                    existingBooking.Status = "Confirmed";
                                    db.Entry(existingBooking).State = System.Data.Entity.EntityState.Modified;
                                }

                                // Creează o înregistrare de plată
                                var payment = new Payment
                                {
                                    BookingId = booking.BookingId,
                                    Amount = booking.TotalAmount,
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

        private bool ProcessPayment(CardDetails cardDetails, decimal amount)
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

        public ActionResult LoginAdmin()
        {
            return View();
        }
        public ActionResult AdminManagment()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(URegisterData registerData)
        {
            if (ModelState.IsValid)
            {
                registerData.UserIp = Request.UserHostAddress;
                registerData.LastLogin = DateTime.Now;
                registerData.Level = 0;

                var registerResponse = _userApi.UserRegister(registerData);

                if (registerResponse.Status)
                {
                    ViewBag.SuccessMessage = true;
                    return View("Registre");
                }
                else
                {
                    ModelState.AddModelError("", registerResponse.StatusMsg);
                }
            }

            return View("Registre", registerData);
        }

        [HttpPost]
        public ActionResult AddToCart(int carId, DateTime pickupDate, DateTime returnDate, decimal totalAmount)
        {
            // Creez un nou booking
            var booking = new Web.Models.Booking
            {
                BookingId = new Random().Next(1000, 9999), // Generare ID temporar
                CarId = carId,
                UserId = Session["Id"] != null ? Convert.ToInt32(Session["Id"]) : 0,
                PickupDate = pickupDate,
                ReturnDate = returnDate,
                TotalAmount = totalAmount,
                Status = "Pending"
            };

            // Salvez booking-ul în sesiune
            Session["CurrentBooking"] = booking;

            // Adaug în coș
            List<Web.Models.Booking> cart = Session["Cart"] as List<Web.Models.Booking>;
            if (cart == null)
            {
                cart = new List<Web.Models.Booking>();
            }
            cart.Add(booking);
            Session["Cart"] = cart;

            return RedirectToAction("Cos");
        }
    }
}