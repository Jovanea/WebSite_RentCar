using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using eUseControl.Domain.Entities.User;
using eUseControl.BusinessLogic;
using eUseControl.BusinessLogic.Interfaces;

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

        public ActionResult Cardetalies()
        {
            return View();
        }

        public ActionResult Login()
        {
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
    }
}