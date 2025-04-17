using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Web;
using System.Web.Mvc;
using eUseControl.BusinessLogic;
using eUseControl.Domain.Entities.User;
using eUseControl.Domain.Entities.Car;
using eUseControl.BusinessLogic.Interfaces;




namespace Web.Controllers
{
    public class LogicController : Controller
    {
        private readonly ISession _session;
        private readonly IUserApi _userApi;
        public LogicController()
        {
            var bl = new BusinessLogic();
            _session = bl.GetSessionBL();
            _userApi = new UserApi();
        }

        // GET: Logic
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
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError("", userLogin.StatusMsg);
                    return View();
                }
            }
            return View();
        }


        public class CarController : Controller
        {
            private readonly ICarSession _carSession;

            public CarController()
            {
                var bl = new BusinessLogic();
                _carSession = bl.GetCarSessionBL();
            }

            public ActionResult Details(int id)
            {
                var carDetails = _carSession.GetCarDetails(id);
                if (carDetails == null)
                {
                    return HttpNotFound(); 
                }
                return View(carDetails);
            }

            public ActionResult Sections()
            {
                var carSections = _carSession.GetCarSections();
                return View(carSections);
            }

            public ActionResult Create()
            {
                return View();
            }

            [HttpPost]
            [ValidateAntiForgeryToken]
            public ActionResult Create(CarDetails carDetails)
            {
                if (ModelState.IsValid)
                {
                    
                    return RedirectToAction("Details", new { id = carDetails.Id }); 
                }

                return View(carDetails);
            }

            public ActionResult Edit(int id)
            {
                var carDetails = _carSession.GetCarDetails(id);
                if (carDetails == null)
                {
                    return HttpNotFound(); 
                }
                return View(carDetails);
            }

            // POST: Car/Edit/5
            [HttpPost]
            [ValidateAntiForgeryToken]
            public ActionResult Edit(int id, CarDetails carDetails)
            {
                if (ModelState.IsValid)
                {

                    return RedirectToAction("Details", new { id = carDetails.Id }); 
                }

                return View(carDetails);
            }

            // GET: Car/Delete/5
            public ActionResult Delete(int id)
            {
                var carDetails = _carSession.GetCarDetails(id);
                if (carDetails == null)
                {
                    return HttpNotFound(); 
                }
                return View(carDetails);
            }

            // POST: Car/Delete/5
            [HttpPost, ActionName("Delete")]
            [ValidateAntiForgeryToken]
            public ActionResult DeleteConfirmed(int id)
            {
                

                return RedirectToAction("Sections"); 
            }
        }
    }
}