using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using eUseControl.Domain.Entities.User;
using eUseControl.BusinessLogic;
using eUseControl.BusinessLogic.Interfaces;
using Web.Models;
using System.Data.Entity;

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
                    Session["Id"] = userLogin.Id;
                    Session["UserName"] = userLogin.Credential;
                    Session["Email"] = login.Credential;
                    Session["Phone"] = userLogin.Phone;

                    // Verifică dacă există un URL de retur
                    if (Request.Form["returnUrl"] != null)
                    {
                        return Redirect(Request.Form["returnUrl"]);
                    }

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
            // Dacă utilizatorul este deja autentificat
            if (Session["Id"] != null)
            {
                return RedirectToAction("Index");
            }

            // Verifică dacă utilizatorul vine de la pagina de coș (pentru plată)
            if (TempData["RedirectToPayment"] != null && (bool)TempData["RedirectToPayment"])
            {
                ViewBag.RedirectToPayment = true;
            }

            // Salvează URL-ul anterior pentru redirecționare după login
            if (TempData["ReturnUrl"] != null)
            {
                ViewBag.ReturnUrl = TempData["ReturnUrl"].ToString();
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
            if (Session["Id"] == null)
            {
                return RedirectToAction("Login");
            }

            return View();
        }

        public ActionResult Review()
        {
            return View();
        }

        public ActionResult Cos()
        {
            if (Session["Id"] == null)
            {
                return RedirectToAction("Login");
            }

            // Ia coșul din sesiune și pune-l în ViewBag
            var cart = Session["Cart"] as List<Web.Models.Booking>;
            ViewBag.CartBookings = cart;

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
            ViewBag.Username = Session["UserName"]; // Adăugăm username-ul pentru afișare

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
                            // Pentru fiecare booking din coș
                            foreach (var booking in cart)
                            {
                                // Actualizează booking-ul
                                booking.Status = "Confirmed";

                                // Dacă booking-ul nu există deja în baza de date, adăugă-l
                                var existingBooking = db.Bookings.Find(booking.BookingId);

                                if (existingBooking != null)
                                {
                                    // Actualizează booking-ul existent
                                    existingBooking.Status = "Confirmed";
                                    db.Entry(existingBooking).State = System.Data.Entity.EntityState.Modified;

                                    // Creează o înregistrare de plată
                                    var payment = new Payment
                                    {
                                        BookingId = booking.BookingId,
                                        Amount = booking.TotalAmount,
                                        PaymentDate = DateTime.Now,
                                        PaymentStatus = "Completed",
                                        TransactionId = Guid.NewGuid().ToString()
                                    };

                                }
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
                        ModelState.AddModelError("", "Plata nu a putut fi procesată. Vă rugăm să încercați din nou.");
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "A apărut o eroare la procesarea plății: " + ex.Message);
                }
            }

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
                    // Setăm datele în sesiune după înregistrare reușită
                    Session["Id"] = registerData.UserName;
                    Session["UserName"] = registerData.UserName;
                    Session["Email"] = registerData.Email;
                    Session["Phone"] = registerData.Phone;

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
        [ValidateAntiForgeryToken]
        public ActionResult AddToCart(int carId, DateTime pickupDate, DateTime returnDate, decimal totalAmount)
        {
            if (Session["Id"] == null)
            {
                TempData["ReturnUrl"] = Url.Action("Cos", "Home");
                return RedirectToAction("Login");
            }

            // Validări pentru datele de rezervare
            if (returnDate <= pickupDate)
            {
                ModelState.AddModelError("", "Data de returnare trebuie să fie mai mare decât data de împrumutare.");
            }
            if (pickupDate < DateTime.Today)
            {
                ModelState.AddModelError("", "Data de început nu poate fi în trecut.");
            }
            if ((returnDate - pickupDate).TotalDays < 1)
            {
                ModelState.AddModelError("", "Perioada de închiriere trebuie să fie de cel puțin o zi.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.CarId = carId;
                return View("Cardetalies");
            }

            // Obține prețul pe zi pentru mașina selectată
            decimal pricePerDay = 0;
            switch (carId)
            {
                case 1: pricePerDay = 60; break;
                case 2: pricePerDay = 100; break;
                case 3: pricePerDay = 600; break;
                case 4: pricePerDay = 500; break;
                case 5: pricePerDay = 700; break;
                case 6: pricePerDay = 2000; break;
                case 7: pricePerDay = 5000; break;
                case 8: pricePerDay = 450; break;
                case 9: pricePerDay = 1500; break;
                case 10: pricePerDay = 1000; break;
                case 11: pricePerDay = 850; break;
                case 12: pricePerDay = 60; break;
                default:
                    TempData["Error"] = "Mașina selectată nu există.";
                    return RedirectToAction("Carsection");
            }

            // Calculează suma totală
            decimal calculatedTotal = pricePerDay * (returnDate - pickupDate).Days;

            // Verifică dacă suma totală calculată se potrivește cu cea primită
            if (Math.Abs(calculatedTotal - totalAmount) > 0.01m)
            {
                TempData["Error"] = "Suma totală calculată nu se potrivește cu cea așteptată.";
                return RedirectToAction("Carsection");
            }

            try
            {
                // Creez un nou booking și îl salvez în baza de date
                using (var db = new ApplicationDbContext())
                {
                    var booking = new Web.Models.Booking
                    {
                        CarId = carId,
                        UserId = (int)Session["Id"],
                        PickupDate = pickupDate,
                        ReturnDate = returnDate,
                        TotalAmount = calculatedTotal,
                        Status = "Confirm"
                    };

                    db.Bookings.Add(booking);
                    db.SaveChanges();

                    // Adaug în coș după ce am salvat în baza de date pentru a avea BookingId
                    List<Web.Models.Booking> cart = Session["Cart"] as List<Web.Models.Booking>;
                    if (cart == null)
                    {
                        cart = new List<Web.Models.Booking>();
                    }
                    cart.Add(booking);
                    Session["Cart"] = cart;

                    // Redirect către pagina Coș
                    return RedirectToAction("Cos", "Home");
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "A apărut o eroare la salvarea rezervării: " + ex.Message;
                return RedirectToAction("Carsection");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateProfile(string UserName, string Email, string Phone)
        {
            if (Session["Id"] == null)
            {
                return RedirectToAction("Login");
            }

            try
            {
                using (var db = new eUseControl.BusinessLogic.Core.DBModel.UserContext())
                {
                    string currentUsername = Session["Id"].ToString();
                    var user = db.Users.FirstOrDefault(u => u.UserName == currentUsername);

                    if (user == null)
                    {
                        TempData["ErrorMessage"] = "Utilizatorul nu a fost găsit.";
                        return RedirectToAction("Profile");
                    }

                    // Verifică dacă noul username există deja (dacă s-a schimbat)
                    if (UserName != currentUsername && db.Users.Any(u => u.UserName == UserName))
                    {
                        TempData["ErrorMessage"] = "Acest nume de utilizator există deja.";
                        return RedirectToAction("Profile");
                    }

                    // Verifică dacă noul email există deja (dacă s-a schimbat)
                    if (Email != user.Email && db.Users.Any(u => u.Email == Email))
                    {
                        TempData["ErrorMessage"] = "Acest email există deja.";
                        return RedirectToAction("Profile");
                    }

                    // Verifică dacă noul numar există deja (dacă s-a schimbat)
                    if (Phone != user.Phone && db.Users.Any(u => u.Phone == Phone))
                    {
                        TempData["ErrorMessage"] = "Acest telefon există deja.";
                        return RedirectToAction("Profile");
                    }

                    // Actualizează datele în baza de date
                    user.UserName = UserName;
                    user.Email = Email;
                    user.Phone = Phone;

                    // Salvează modificările în baza de date
                    db.SaveChanges();

                    // Actualizează sesiunea
                    Session["Id"] = UserName;
                    Session["UserName"] = UserName;
                    Session["Email"] = Email;
                    Session["Phone"] = Phone;

                    TempData["SuccessMessage"] = "Profilul a fost actualizat cu succes!";
                    return RedirectToAction("Profile");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating profile: {ex.Message}");
                TempData["ErrorMessage"] = "A apărut o eroare la actualizarea profilului: " + ex.Message;
                return RedirectToAction("Profile");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangePassword(string CurrentPassword, string NewPassword, string ConfirmPassword)
        {
            if (Session["Id"] == null)
            {
                return RedirectToAction("Login");
            }

            if (NewPassword != ConfirmPassword)
            {
                TempData["ErrorMessage"] = "Parolele noi nu coincid.";
                return RedirectToAction("Profile");
            }

            try
            {
                using (var db = new eUseControl.BusinessLogic.Core.DBModel.UserContext())
                {
                    string username = Session["Id"].ToString();
                    // Get the user by username (stored in Session["Id"])
                    var user = db.Users.FirstOrDefault(u => u.UserName == username);

                    if (user == null)
                    {
                        TempData["ErrorMessage"] = "Utilizatorul nu a fost găsit.";
                        return RedirectToAction("Profile");
                    }

                    // Verify current password
                    if (user.Password != CurrentPassword)
                    {
                        TempData["ErrorMessage"] = "Parola curentă este incorectă.";
                        return RedirectToAction("Profile");
                    }

                    // Update password
                    user.Password = NewPassword;

                    try
                    {
                        // Save changes to database
                        db.SaveChanges();
                        TempData["SuccessMessage"] = "Parola a fost schimbată cu succes!";
                    }
                    catch (Exception dbEx)
                    {
                        System.Diagnostics.Debug.WriteLine($"Database error: {dbEx.Message}");
                        System.Diagnostics.Debug.WriteLine($"Stack trace: {dbEx.StackTrace}");
                        TempData["ErrorMessage"] = "Eroare la salvarea în baza de date: " + dbEx.Message;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"General error: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                TempData["ErrorMessage"] = "A apărut o eroare la schimbarea parolei. " + ex.Message;
            }

            return RedirectToAction("Profile");
        }

        public ActionResult Logout()
        {
            // Clear all session variables
            Session.Clear();
            Session.Abandon();

            // Redirect to login page
            return RedirectToAction("Login");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RemoveFromCart(int bookingId)
        {
            var cart = Session["Cart"] as List<Web.Models.Booking>;
            if (cart != null)
            {
                var bookingToRemove = cart.FirstOrDefault(b => b.BookingId == bookingId);
                if (bookingToRemove != null)
                {
                    cart.Remove(bookingToRemove);
                    Session["Cart"] = cart;
                }
            }
            return RedirectToAction("Cos");
        }
    }
}