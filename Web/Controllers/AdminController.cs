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

            if (Session["Level"] == null || (int)Session["Level"] == 0)
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

        public ActionResult Details(int id)
        {
            try
            {
                var car = _adminApi.GetCarById(id);
                if (car == null)
                {
                    return HttpNotFound();
                }
                return View("Details", car);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in AdminController Details: {ex.Message}");
                ViewBag.ErrorMessage = "Error loading car details: " + ex.Message;
                return View("Details", null);
            }
        }

        public ActionResult Create()
        {
            var model = new Car
            {
                PricePerDay = 0,
                Year = DateTime.Now.Year,
                IsAvailable = true,
                Stock = 1
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Car car)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("[DEBUG] AdminController.Create POST called.");

                if (ModelState.IsValid)
                {
                    if (_adminApi.CreateCar(car))
                    {
                        TempData["SuccessMessage"] = "Car created successfully!";

                        return RedirectToAction("Cars");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Could not create car details.");
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Please check the car details for validation errors.");
                }

                System.Diagnostics.Debug.WriteLine($"[DEBUG] ModelState.IsValid before returning view: {ModelState.IsValid}");
                if (!ModelState.IsValid)
                {
                    System.Diagnostics.Debug.WriteLine("[DEBUG] ModelState errors:");
                    foreach (var key in ModelState.Keys)
                    {
                        var state = ModelState[key];
                        if (state.Errors.Any())
                        {
                            System.Diagnostics.Debug.WriteLine($"[DEBUG] Key: {key}");
                            foreach (var error in state.Errors)
                            {
                                System.Diagnostics.Debug.WriteLine($"[DEBUG] - {error.ErrorMessage}");
                            }
                        }
                    }
                }

                return View(car);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error: " + ex.Message);
                return View(car);
            }
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
        public ActionResult Edit(Car car)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (_adminApi.UpdateCar(car))
                    {
                        TempData["SuccessMessage"] = "Car updated successfully!";

                        return RedirectToAction("Cars");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Failed to update car. Please check input data and try again.");
                        return View(car);
                    }
                }
                return View(car);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error: " + ex.Message);
                return View(car);
            }
        }

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