using System;
using System.Web.Mvc;
using Web.Interfaces;
using Web.BusinessLogic;
using eUseControl.Domain.Entities.Car;
using eUseControl.BusinessLogic.Models;
using eUseControl.BusinessLogic.DBModel;
using System.Linq;
using System.Collections.Generic;

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
                if (TempData["ErrorMessage"] != null)
                {
                    ViewBag.ErrorMessage = TempData["ErrorMessage"];
                }
                if (TempData["SuccessMessage"] != null)
                {
                    ViewBag.SuccessMessage = TempData["SuccessMessage"];
                }
                return View(cars);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Error loading cars: " + ex.Message;
                return View(new List<Car>());
            }
        }

        public ActionResult Create()
        {
            var model = new Car
            {
                PricePerDay = 0.0m,
                Year = DateTime.Now.Year,
                IsAvailable = true,
                Stock = 1
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Car car, System.Web.HttpPostedFileBase mainImage)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (mainImage != null)
                    {
                        var imageUpload = new ImageUpload
                        {
                            FileName = mainImage.FileName,
                            ContentType = mainImage.ContentType,
                            Content = new byte[mainImage.ContentLength]
                        };
                        mainImage.InputStream.Read(imageUpload.Content, 0, mainImage.ContentLength);

                        if (_adminApi.CreateCar(car) && _adminApi.SaveCarImage(car.CarId, imageUpload, "main"))
                        {
                            TempData["SuccessMessage"] = "Car created successfully!";
                            return RedirectToAction("Cars");
                        }
                    }
                    else if (_adminApi.CreateCar(car))
                    {
                        TempData["SuccessMessage"] = "Car created successfully!";
                        return RedirectToAction("Cars");
                    }
                    ModelState.AddModelError("", "Could not create car.");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error: " + ex.Message);
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
        public ActionResult Edit(Car car, System.Web.HttpPostedFileBase mainImage)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (mainImage != null)
                    {
                        var imageUpload = new ImageUpload
                        {
                            FileName = mainImage.FileName,
                            ContentType = mainImage.ContentType,
                            Content = new byte[mainImage.ContentLength]
                        };
                        mainImage.InputStream.Read(imageUpload.Content, 0, mainImage.ContentLength);

                        if (_adminApi.UpdateCar(car) && _adminApi.SaveCarImage(car.CarId, imageUpload, "main"))
                        {
                            TempData["SuccessMessage"] = "Car updated successfully!";
                            return RedirectToAction("Cars");
                        }
                    }
                    else if (_adminApi.UpdateCar(car))
                    {
                        TempData["SuccessMessage"] = "Car updated successfully!";
                        return RedirectToAction("Cars");
                    }
                    ModelState.AddModelError("", "Could not update car.");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error: " + ex.Message);
            }
            return View(car);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            try
            {
                if (_adminApi.DeleteCar(id))
                {
                    TempData["SuccessMessage"] = "Car deleted successfully!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Cannot delete car because it has associated bookings.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error deleting car: " + ex.Message;
            }
            return RedirectToAction("Cars");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ToggleAvailability(int id)
        {
            try
            {
                if (_adminApi.ToggleCarAvailability(id))
                {
                    TempData["SuccessMessage"] = "Car availability updated successfully!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Could not update car availability.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error updating car availability: " + ex.Message;
            }
            return RedirectToAction("Cars");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateStock(int id, int stockChange)
        {
            try
            {
                if (_adminApi.UpdateCarStock(id, stockChange))
                {
                    TempData["SuccessMessage"] = "Car stock updated successfully!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Could not update car stock.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error updating car stock: " + ex.Message;
            }
            return RedirectToAction("Cars");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateUserToAdmin(string email)
        {
            if (_adminApi.UpdateUserToAdmin(email))
            {
                TempData["SuccessMessage"] = "User promoted to admin successfully!";
            }
            else
            {
                TempData["ErrorMessage"] = "Could not promote user to admin.";
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
                    TempData["SuccessMessage"] = "Admin account created successfully!";
                    return RedirectToAction("LoginAdmin", "Home");
                }
                ModelState.AddModelError("", "Could not create admin account.");
            }
            return View("Registre", userData);
        }
    }
}