using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using eUseControl.Domain.Entities.User;
using eUseControl.BusinessLogic;
using eUseControl.BusinessLogic.Interfaces;
using eUseControl.BusinessLogic.DBModel;
using System.Data.Entity;
using Web.Interfaces;
using Web.BusinessLogic;

namespace Web.Controllers
{
    public class HomeController : BaseController
    {
        private readonly IUserApi _userApi;
        private readonly ICarApi _carApi;
        private readonly IBookingApi _bookingApi;
        private readonly IPaymentApi _paymentApi;

        public HomeController()
        {
            _userApi = new UserApi();
            _carApi = new CarApi();
            _bookingApi = new BookingApi();
            _paymentApi = new PaymentApi();
        }

        public ActionResult Index()
        {
            SessionStatus();
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
                    Session["Id"] = userLogin.Id;
                    Session["UserName"] = userLogin.Credential;
                    Session["Email"] = login.Credential;
                    Session["Phone"] = userLogin.Phone;
                    Session.Timeout = 60;

                    var session = _userApi.GetSessionBL();
                    string secureToken = session.CreateCookie(data);

                    Response.Cookies.Add(new HttpCookie("X-KEY", secureToken)
                    {
                        Expires = DateTime.Now.AddHours(1),
                        HttpOnly = true
                    });

                    var userData = new
                    {
                        Id = userLogin.Id,
                        UserName = userLogin.Credential,
                        Email = login.Credential,
                        Phone = userLogin.Phone,
                        LastLogin = DateTime.Now
                    };

                    Response.Cookies.Add(new HttpCookie("user_session", Newtonsoft.Json.JsonConvert.SerializeObject(userData))
                    {
                        Expires = DateTime.Now.AddMinutes(60),
                        HttpOnly = true
                    });

                    Response.Cookies.Add(new HttpCookie("last_user", Newtonsoft.Json.JsonConvert.SerializeObject(userData))
                    {
                        Expires = DateTime.Now.AddYears(1),
                        HttpOnly = true
                    });

                    if (Request.Form["returnUrl"] != null)
                    {
                        return Redirect(Request.Form["returnUrl"]);
                    }

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
            try
            {
                var cars = _carApi.GetAllCars();
                return View(cars);
            }
            catch (Exception ex)
            {
                // Tratăm excepția, dar nu afișăm mesaje de debug
                return View(new List<eUseControl.Domain.Entities.Car.CarDetails>());
            }
        }

        public ActionResult Cardetalies(int id = 1)
        {
            var car = _carApi.GetCarById(id);
            if (car == null)
                return HttpNotFound();

            ViewBag.CarId = id;
            return View(car);
        }

        public ActionResult Login()
        {
            SessionStatus();

            if (Session["Id"] != null)
            {
                // If there was a pending booking, continue with it
                if (TempData["BookingRedirect"] != null && (bool)TempData["BookingRedirect"])
                {
                    int carId = (int)TempData["CarId"];
                    DateTime pickupDate = (DateTime)TempData["PickupDate"];
                    DateTime returnDate = (DateTime)TempData["ReturnDate"];
                    int totalAmount = (int)TempData["TotalAmount"];

                    return RedirectToAction("Cardetalies", new { id = carId });
                }
                
                return RedirectToAction("Profile", "Home");
            }

            var xKeyCookie = Request.Cookies["X-KEY"];
            if (xKeyCookie != null)
            {
                var session = _userApi.GetSessionBL();
                var userData = session.GetUserByCookie(xKeyCookie.Value);

                if (userData != null)
                {
                    var userApi = new UserApi();
                    var userLogin = userApi.UserLogin(userData);

                    if (userLogin.Status)
                    {
                        Session["Id"] = userLogin.Id;
                        Session["UserName"] = userLogin.Credential;
                        Session["Email"] = userData.Credential;
                        Session["Phone"] = userLogin.Phone;

                        return RedirectToAction("Profile", "Home");
                    }
                }
            }

            var sessionCookie = Request.Cookies["user_session"];
            if (sessionCookie != null)
            {
                try
                {
                    var userData = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(sessionCookie.Value);
                    if (userData != null)
                    {
                        Session["Id"] = Convert.ToInt32(userData.Id);
                        Session["UserName"] = userData.UserName;
                        Session["Email"] = userData.Email;
                        Session["Phone"] = userData.Phone;
                        return RedirectToAction("Profile", "Home");
                    }
                }
                catch
                {
                    Response.Cookies.Add(new HttpCookie("user_session") { Expires = DateTime.Now.AddDays(-1) });
                }
            }

            if (TempData["RedirectToPayment"] != null && (bool)TempData["RedirectToPayment"])
            {
                ViewBag.RedirectToPayment = true;
            }

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
            SessionStatus();

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

            // Show success/error messages if they exist
            if (TempData["SuccessMessage"] != null)
            {
                ViewBag.SuccessMessage = TempData["SuccessMessage"];
            }
            
            if (TempData["ErrorMessage"] != null)
            {
                ViewBag.ErrorMessage = TempData["ErrorMessage"];
            }

            var cart = Session["Cart"] as List<eUseControl.BusinessLogic.DBModel.Booking>;
            
            // If cart doesn't exist at all (first visit), initialize it from database
            if (cart == null)
            {
                int userId = GetCurrentUserId();
                if (userId > 0)
                {
                    // Get only pending bookings from database
                    var bookings = _bookingApi.GetUserBookings(userId)
                        .Where(b => b.Status == "Pending")
                        .ToList();
                    
                    if (bookings.Count > 0)
                    {
                        cart = bookings;
                        Session["Cart"] = cart;
                    }
                    else
                    {
                        // No bookings found, set empty cart
                        cart = new List<eUseControl.BusinessLogic.DBModel.Booking>();
                        Session["Cart"] = cart;
                    }
                }
                else
                {
                    // User not found, return empty cart
                    cart = new List<eUseControl.BusinessLogic.DBModel.Booking>();
                    Session["Cart"] = cart;
                }
            }
            
            // Set cart for the view
            ViewBag.CartBookings = cart;
            
            // Calculate total if needed
            if (cart != null && cart.Count > 0)
            {
                ViewBag.CartTotal = cart.Sum(b => b.TotalAmount);
            }
            else
            {
                ViewBag.CartTotal = 0;
            }

            return View();
        }

        private int GetCurrentUserId()
        {
            // Helper method to get current user ID
            int userId = 0;
            
            if (Session["Id"] is int)
            {
                userId = (int)Session["Id"];
            }
            else if (int.TryParse(Session["Id"]?.ToString(), out int parsedId))
            {
                userId = parsedId;
            }
            else
            {
                // Try to get user ID from database
                using (var db = new eUseControl.BusinessLogic.DBModel.UserContext())
                {
                    var username = Session["Id"]?.ToString();
                    if (!string.IsNullOrEmpty(username))
                    {
                        var user = db.Users.FirstOrDefault(u => u.UserName == username);
                        if (user != null)
                        {
                            userId = user.Id;
                        }
                    }
                }
            }
            
            return userId;
        }

        public ActionResult Pay()
        {
            if (Session["Id"] == null)
            {
                TempData["RedirectToPayment"] = true;
                return RedirectToAction("Login", "Home");
            }

            var cart = Session["Cart"] as List<eUseControl.BusinessLogic.DBModel.Booking>;
            if (cart == null || cart.Count == 0)
            {
                // Try to get pending bookings from database
                int userId;
                if (Session["Id"] is int)
                {
                    userId = (int)Session["Id"];
                }
                else if (int.TryParse(Session["Id"].ToString(), out int parsedId))
                {
                    userId = parsedId;
                }
                else
                {
                    // If can't parse as int, need to get user ID from database
                    using (var db = new eUseControl.BusinessLogic.DBModel.UserContext())
                    {
                        var username = Session["Id"].ToString();
                        var user = db.Users.FirstOrDefault(u => u.UserName == username);
                        if (user == null)
                        {
                            return RedirectToAction("Carsection", "Home");
                        }
                        userId = user.Id;
                    }
                }
                
                var pendingBookings = _bookingApi.GetUserBookings(userId)
                    .Where(b => b.Status == "Pending")
                    .ToList();
                
                if (pendingBookings.Count > 0)
                {
                    cart = pendingBookings;
                    Session["Cart"] = cart;
                }
                else
                {
                    return RedirectToAction("Carsection", "Home");
                }
            }

            // Calculate the total and handle conversion from int to decimal if needed
            decimal totalAmount = cart.Sum(b => (decimal)b.TotalAmount);
            ViewBag.TotalAmount = totalAmount;
            ViewBag.Username = Session["UserName"];

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Pay(CardDetails cardDetails)
        {
            if (!ModelState.IsValid)
                return View();

            int userId = GetCurrentUserId();
            var booking = _bookingApi.GetUserBookings(userId)
                .FirstOrDefault(b => b.Status == "Pending");

            if (booking == null)
                return RedirectToAction("Index");

            try
            {
                if (_paymentApi.ProcessPayment(cardDetails, booking.TotalAmount))
                {
                    // Update booking status
                    booking.Status = "Paid";
                    if (_bookingApi.UpdateBooking(booking))
                    {
                        // Clear the cart
                        Session["Cart"] = new List<Booking>();
                        
                        // Store successful payment info
                        TempData["PaymentSuccess"] = true;
                        TempData["PaymentAmount"] = booking.TotalAmount;
                        TempData["BookingId"] = booking.BookingId;
                        
                        return RedirectToAction("Success", "Payment");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Plata a fost procesată, dar actualizarea rezervării a eșuat.");
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Plata nu a putut fi procesată.");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Payment error: {ex.Message}");
                ModelState.AddModelError("", $"Eroare la procesarea plății: {ex.Message}");
            }
            
            return View();
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
            // Check if user is logged in
            if (Session["Id"] == null)
            {
                // Store details in TempData to persist across redirect
                TempData["BookingRedirect"] = true;
                TempData["CarId"] = carId;
                TempData["PickupDate"] = pickupDate;
                TempData["ReturnDate"] = returnDate;
                TempData["TotalAmount"] = totalAmount;
                
                // Redirect to login
                return RedirectToAction("Login");
            }

            try 
            {
                if (!ModelState.IsValid)
                    return View();

                // Use ToString() first and then try to parse as int to handle numeric or string IDs
                int userId;
                if (Session["Id"] is int)
                {
                    userId = (int)Session["Id"];
                }
                else if (int.TryParse(Session["Id"].ToString(), out int parsedId))
                {
                    userId = parsedId;
                }
                else
                {
                    // If can't parse as int, need to get user ID from database
                    using (var db = new eUseControl.BusinessLogic.DBModel.UserContext())
                    {
                        var username = Session["Id"].ToString();
                        var user = db.Users.FirstOrDefault(u => u.UserName == username);
                        if (user == null)
                        {
                            TempData["ErrorMessage"] = "Utilizatorul nu a fost găsit.";
                            return RedirectToAction("Login");
                        }
                        userId = user.Id;
                    }
                }

                // Create booking with user ID - convert decimal to int
                var booking = new Booking
                {
                    CarId = carId,
                    UserId = userId,
                    PickupDate = pickupDate,
                    ReturnDate = returnDate,
                    TotalAmount = (int)totalAmount, // Convert decimal to int
                    Status = "Confirm"
                };

                if (_bookingApi.CreateBooking(booking))
                {
                    // Add to session cart
                    var cart = Session["Cart"] as List<Booking>;
                    if (cart == null)
                    {
                        cart = new List<Booking>();
                    }
                    cart.Add(booking);
                    Session["Cart"] = cart;
                    
                    return RedirectToAction("Pay", "Home");
                }

                TempData["ErrorMessage"] = "Nu s-a putut crea rezervarea.";
                return RedirectToAction("Cardetalies", new { id = carId });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in AddToCart: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                TempData["ErrorMessage"] = "A apărut o eroare la crearea rezervării: " + ex.Message;
                return RedirectToAction("Cardetalies", new { id = carId });
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
                using (var db = new eUseControl.BusinessLogic.DBModel.UserContext())
                {
                    string currentUsername = Session["Id"].ToString();
                    var user = db.Users.FirstOrDefault(u => u.UserName == currentUsername);

                    if (user == null)
                    {
                        TempData["ErrorMessage"] = "Utilizatorul nu a fost găsit.";
                        return RedirectToAction("Profile");
                    }

                    if (UserName != currentUsername && db.Users.Any(u => u.UserName == UserName))
                    {
                        TempData["ErrorMessage"] = "Acest nume de utilizator există deja.";
                        return RedirectToAction("Profile");
                    }
                    if (Email != user.Email && db.Users.Any(u => u.Email == Email))
                    {
                        TempData["ErrorMessage"] = "Acest email există deja.";
                        return RedirectToAction("Profile");
                    }

                    if (Phone != user.Phone && db.Users.Any(u => u.Phone == Phone))
                    {
                        TempData["ErrorMessage"] = "Acest telefon există deja.";
                        return RedirectToAction("Profile");
                    }

                    user.UserName = UserName;
                    user.Email = Email;
                    user.Phone = Phone;

                    db.SaveChanges();

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
                using (var db = new eUseControl.BusinessLogic.DBModel.UserContext())
                {
                    string username = Session["Id"].ToString();
                    var user = db.Users.FirstOrDefault(u => u.UserName == username);

                    if (user == null)
                    {
                        TempData["ErrorMessage"] = "Utilizatorul nu a fost găsit.";
                        return RedirectToAction("Profile");
                    }

                    if (!eUseControl.BusinessLogic.Core.PasswordHasher.VerifyPassword(CurrentPassword, user.Password))
                    {
                        TempData["ErrorMessage"] = "Parola curentă este incorectă.";
                        return RedirectToAction("Profile");
                    }

                    user.Password = eUseControl.BusinessLogic.Core.PasswordHasher.HashPassword(NewPassword);

                    try
                    {
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
            Session.Clear();
            Session.Abandon();

            Response.Cookies.Add(new HttpCookie("user_session") { Expires = DateTime.Now.AddDays(-1) });

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RemoveFromCart(int bookingId)
        {
            try
            {
                // First remove from session cart
                var cart = Session["Cart"] as List<eUseControl.BusinessLogic.DBModel.Booking>;
                if (cart != null)
                {
                    var bookingToRemove = cart.FirstOrDefault(b => b.BookingId == bookingId);
                    if (bookingToRemove != null)
                    {
                        cart.Remove(bookingToRemove);
                        Session["Cart"] = cart;
                    }
                }
                
                // Then cancel in the database to prevent reloading
                _bookingApi.CancelBooking(bookingId);
                
                // Return success
                TempData["SuccessMessage"] = "Produsul a fost șters din coș.";
                return RedirectToAction("Cos");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error removing from cart: {ex.Message}");
                TempData["ErrorMessage"] = "A apărut o eroare la ștergerea produsului din coș.";
                return RedirectToAction("Cos");
            }
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ClearCart()
        {
            try
            {
                // Get user ID
                int userId = 0;
                if (Session["Id"] is int)
                {
                    userId = (int)Session["Id"];
                }
                else if (int.TryParse(Session["Id"]?.ToString(), out int parsedId))
                {
                    userId = parsedId;
                }
                else
                {
                    // Try to get user ID from database
                    using (var db = new eUseControl.BusinessLogic.DBModel.UserContext())
                    {
                        var username = Session["Id"]?.ToString();
                        if (!string.IsNullOrEmpty(username))
                        {
                            var user = db.Users.FirstOrDefault(u => u.UserName == username);
                            if (user != null)
                            {
                                userId = user.Id;
                            }
                        }
                    }
                }
                
                if (userId > 0)
                {
                    // Cancel all pending bookings for this user
                    _bookingApi.CancelBookingByUserAndStatus(userId, "Pending");
                    
                    // Clear the session cart
                    Session["Cart"] = new List<eUseControl.BusinessLogic.DBModel.Booking>();
                    
                    TempData["SuccessMessage"] = "Coșul a fost golit cu succes.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Nu s-a putut identifica utilizatorul.";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error clearing cart: {ex.Message}");
                TempData["ErrorMessage"] = "A apărut o eroare la golirea coșului.";
            }
            
            return RedirectToAction("Cos");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LoginAdmin(AdminLogin login)
        {
            if (ModelState.IsValid)
            {
                using (var db = new eUseControl.BusinessLogic.DBModel.UserContext())
                {
                    var admin = db.Users.FirstOrDefault(u =>
                        (u.Email == login.Username || u.UserName == login.Username) &&
                        u.Level == 1);

                    if (admin != null && eUseControl.BusinessLogic.Core.PasswordHasher.VerifyPassword(login.Password, admin.Password))
                    {
                        Session["Id"] = admin.Id;
                        Session["UserName"] = admin.UserName;
                        Session["Email"] = admin.Email;
                        Session["IsAdmin"] = true;
                        Session.Timeout = 60;

                        return RedirectToAction("Cars", "Admin");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Credențiale incorecte sau nu aveți drepturi de administrator.");
                    }
                }
            }
            return View(login);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateAdmin(URegisterData registerData)
        {
            if (ModelState.IsValid)
            {
                registerData.UserIp = Request.UserHostAddress;
                registerData.LastLogin = DateTime.Now;
                registerData.Level = 1;

                var registerResponse = _userApi.UserRegister(registerData);

                if (registerResponse.Status)
                {
                    TempData["SuccessMessage"] = "Cont de administrator creat cu succes!";
                    return RedirectToAction("LoginAdmin");
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
        public ActionResult UpdateUserToAdmin(string email)
        {
            try
            {
                using (var db = new eUseControl.BusinessLogic.DBModel.UserContext())
                {
                    var user = db.Users.FirstOrDefault(u => u.Email == email);
                    if (user != null)
                    {
                        user.Level = 1;
                        db.SaveChanges();
                        TempData["SuccessMessage"] = "Utilizatorul a fost promovat la administrator cu succes!";
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Utilizatorul nu a fost găsit.";
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "A apărut o eroare: " + ex.Message;
            }
            return RedirectToAction("LoginAdmin");
        }
    }
}