using System;
using System.Linq;
using System.Web.Mvc;
using eUseControl.BusinessLogic.DBModel;
using System.Data.Entity;
using System.IO;
using System.Web;
using System.Collections.Generic;

namespace Web.Controllers
{
    public class AdminController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);

            // Verifică dacă utilizatorul este admin
            if (Session["IsAdmin"] == null || !(bool)Session["IsAdmin"])
            {
                System.Diagnostics.Debug.WriteLine("Acces refuzat la Admin: IsAdmin=" + (Session["IsAdmin"] != null ? Session["IsAdmin"].ToString() : "null"));
                filterContext.Result = new RedirectResult("~/Home/LoginAdmin");
                return;
            }

            System.Diagnostics.Debug.WriteLine("Acces permis la Admin: IsAdmin=" + Session["IsAdmin"]);
        }

        // GET: Admin/Cars
        public ActionResult Cars()
        {
            try
            {
                var cars = db.Cars.ToList();
                return View(cars);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Eroare la încărcarea mașinilor: " + ex.Message);
                ViewBag.ErrorMessage = "Eroare la încărcarea mașinilor: " + ex.Message;
                return View(new List<Car>());
            }
        }

        // GET: Admin/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Admin/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Car car, HttpPostedFileBase mainImage, HttpPostedFileBase interiorImage, HttpPostedFileBase exteriorImage)
        {
            if (ModelState.IsValid)
            {
                // Handle image uploads
                if (mainImage != null && mainImage.ContentLength > 0)
                {
                    var fileName = Path.GetFileName(mainImage.FileName);
                    var path = Path.Combine(Server.MapPath("~/Content/images/cars"), fileName);
                    mainImage.SaveAs(path);
                    car.MainImageUrl = "/Content/images/cars/" + fileName;
                }

                if (interiorImage != null && interiorImage.ContentLength > 0)
                {
                    var fileName = Path.GetFileName(interiorImage.FileName);
                    var path = Path.Combine(Server.MapPath("~/Content/images/cars"), fileName);
                    interiorImage.SaveAs(path);
                    car.InteriorImageUrl = "/Content/images/cars/" + fileName;
                }

                if (exteriorImage != null && exteriorImage.ContentLength > 0)
                {
                    var fileName = Path.GetFileName(exteriorImage.FileName);
                    var path = Path.Combine(Server.MapPath("~/Content/images/cars"), fileName);
                    exteriorImage.SaveAs(path);
                    car.ExteriorImageUrl = "/Content/images/cars/" + fileName;
                }

                db.Cars.Add(car);
                db.SaveChanges();
                return RedirectToAction("Cars");
            }

            return View(car);
        }

        // GET: Admin/Edit/5
        public ActionResult Edit(int id)
        {
            var car = db.Cars.Find(id);
            if (car == null)
            {
                return HttpNotFound();
            }
            return View(car);
        }

        // POST: Admin/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Car car, HttpPostedFileBase mainImage, HttpPostedFileBase interiorImage, HttpPostedFileBase exteriorImage)
        {
            if (ModelState.IsValid)
            {
                var existingCar = db.Cars.Find(car.CarId);
                if (existingCar == null)
                {
                    return HttpNotFound();
                }

                // Handle image uploads
                if (mainImage != null && mainImage.ContentLength > 0)
                {
                    var fileName = Path.GetFileName(mainImage.FileName);
                    var path = Path.Combine(Server.MapPath("~/Content/images/cars"), fileName);
                    mainImage.SaveAs(path);
                    existingCar.MainImageUrl = "/Content/images/cars/" + fileName;
                }

                if (interiorImage != null && interiorImage.ContentLength > 0)
                {
                    var fileName = Path.GetFileName(interiorImage.FileName);
                    var path = Path.Combine(Server.MapPath("~/Content/images/cars"), fileName);
                    interiorImage.SaveAs(path);
                    existingCar.InteriorImageUrl = "/Content/images/cars/" + fileName;
                }

                if (exteriorImage != null && exteriorImage.ContentLength > 0)
                {
                    var fileName = Path.GetFileName(exteriorImage.FileName);
                    var path = Path.Combine(Server.MapPath("~/Content/images/cars"), fileName);
                    exteriorImage.SaveAs(path);
                    existingCar.ExteriorImageUrl = "/Content/images/cars/" + fileName;
                }

                // Update other properties
                existingCar.Brand = car.Brand;
                existingCar.Model = car.Model;
                existingCar.Year = car.Year;
                existingCar.PricePerDay = car.PricePerDay;
                existingCar.Transmission = car.Transmission;
                existingCar.FuelType = car.FuelType;
                existingCar.Horsepower = car.Horsepower;
                existingCar.Seats = car.Seats;
                existingCar.Category = car.Category;
                existingCar.IsAvailable = car.IsAvailable;
                existingCar.Description = car.Description;
                existingCar.Engine = car.Engine;
                existingCar.Torque = car.Torque;
                existingCar.Acceleration = car.Acceleration;
                existingCar.TopSpeed = car.TopSpeed;
                existingCar.FuelConsumption = car.FuelConsumption;

                db.Entry(existingCar).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Cars");
            }

            return View(car);
        }

        // GET: Admin/Delete/5
        public ActionResult Delete(int id)
        {
            var car = db.Cars.Find(id);
            if (car == null)
            {
                return HttpNotFound();
            }
            return View(car);
        }

        // POST: Admin/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var car = db.Cars.Find(id);
            if (car == null)
            {
                return HttpNotFound();
            }

            // Delete associated images
            if (!string.IsNullOrEmpty(car.MainImageUrl))
            {
                var mainImagePath = Server.MapPath("~" + car.MainImageUrl);
                if (System.IO.File.Exists(mainImagePath))
                {
                    System.IO.File.Delete(mainImagePath);
                }
            }

            if (!string.IsNullOrEmpty(car.InteriorImageUrl))
            {
                var interiorImagePath = Server.MapPath("~" + car.InteriorImageUrl);
                if (System.IO.File.Exists(interiorImagePath))
                {
                    System.IO.File.Delete(interiorImagePath);
                }
            }

            if (!string.IsNullOrEmpty(car.ExteriorImageUrl))
            {
                var exteriorImagePath = Server.MapPath("~" + car.ExteriorImageUrl);
                if (System.IO.File.Exists(exteriorImagePath))
                {
                    System.IO.File.Delete(exteriorImagePath);
                }
            }

            db.Cars.Remove(car);
            db.SaveChanges();
            return RedirectToAction("Cars");
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