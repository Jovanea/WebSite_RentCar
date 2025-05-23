using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using eUseControl.Domain.Entities.Car;
using eUseControl.BusinessLogic.Interfaces;
using eUseControl.BusinessLogic;

namespace Web.Controllers
{
    public class CarController : Controller
    {
        private readonly ICarSession _carSession;

        public CarController(ICarSession carSession)
        {
            _carSession = carSession;
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

        public ActionResult Delete(int id)
        {
            var carDetails = _carSession.GetCarDetails(id);
            if (carDetails == null)
            {
                return HttpNotFound();
            }
            return View(carDetails);
        }


        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {

            return RedirectToAction("Sections");
        }

        public ActionResult Sections()
        {
            return View();
        }
    }
}