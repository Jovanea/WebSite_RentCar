using System;
using System.Web.Mvc;
using Web.Interfaces;
using Web.BusinessLogic;
using eUseControl.Domain.Entities.Car;
using eUseControl.BusinessLogic.Models;

namespace Web.Controllers
{
    public class AdminController : Controller
    {
        private readonly IAdminApi _adminApi;

        public AdminController()
        {
            _adminApi = new AdminApi();
        }

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);

            if (Session["IsAdmin"] == null || !(bool)Session["IsAdmin"])
            {
                filterContext.Result = new RedirectResult("~/Home/LoginAdmin");
                return;
            }
        }

        public ActionResult Cars()
        {
            try
            {
                var cars = _adminApi.GetAllCars();
                return View(cars);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Eroare la încărcarea mașinilor: " + ex.Message;
                return View(new System.Collections.Generic.List<CarDetails>());
            }
        }

        public ActionResult Create()
        {
            return View();
        }

        private ImageUpload ConvertToImageUpload(System.Web.HttpPostedFileBase file)
        {
            if (file == null) return null;
            using (var ms = new System.IO.MemoryStream())
            {
                file.InputStream.CopyTo(ms);
                return new ImageUpload
                {
                    FileName = file.FileName,
                    Content = ms.ToArray(),
                    ContentType = file.ContentType
                };
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(CarDetails car, System.Web.HttpPostedFileBase mainImage, System.Web.HttpPostedFileBase interiorImage, System.Web.HttpPostedFileBase exteriorImage)
        {
            if (ModelState.IsValid)
            {
                var mainImg = ConvertToImageUpload(mainImage);
                var intImg = ConvertToImageUpload(interiorImage);
                var extImg = ConvertToImageUpload(exteriorImage);
                if (_adminApi.CreateCar(car, mainImg, intImg, extImg))
                {
                    return RedirectToAction("Cars");
                }
                ModelState.AddModelError("", "Nu s-a putut crea mașina.");
            }
            return View(car);
        }

        public ActionResult Edit(int id)
        {
            var car = _adminApi.GetCarById(id);
            if (car == null)
            {
                return HttpNotFound();
            }
            return View(car);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(CarDetails car, System.Web.HttpPostedFileBase mainImage, System.Web.HttpPostedFileBase interiorImage, System.Web.HttpPostedFileBase exteriorImage)
        {
            if (ModelState.IsValid)
            {
                var mainImg = ConvertToImageUpload(mainImage);
                var intImg = ConvertToImageUpload(interiorImage);
                var extImg = ConvertToImageUpload(exteriorImage);
                if (_adminApi.UpdateCar(car, mainImg, intImg, extImg))
                {
                    return RedirectToAction("Cars");
                }
                ModelState.AddModelError("", "Nu s-a putut actualiza mașina.");
            }
            return View(car);
        }

        public ActionResult Delete(int id)
        {
            var car = _adminApi.GetCarById(id);
            if (car == null)
            {
                return HttpNotFound();
            }
            return View(car);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            if (_adminApi.DeleteCar(id))
            {
                return RedirectToAction("Cars");
            }
            return HttpNotFound();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateUserToAdmin(string email)
        {
            if (_adminApi.UpdateUserToAdmin(email))
            {
                TempData["SuccessMessage"] = "Utilizatorul a fost promovat la administrator cu succes!";
            }
            else
            {
                TempData["ErrorMessage"] = "Nu s-a putut promova utilizatorul la administrator.";
            }
            return RedirectToAction("LoginAdmin", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateAdmin(UserData userData)
        {
            if (ModelState.IsValid)
            {
                if (_adminApi.CreateAdmin(userData))
                {
                    TempData["SuccessMessage"] = "Cont de administrator creat cu succes!";
                    return RedirectToAction("LoginAdmin", "Home");
                }
                ModelState.AddModelError("", "Nu s-a putut crea contul de administrator.");
            }
            return View("Registre", userData);
        }
    }
}