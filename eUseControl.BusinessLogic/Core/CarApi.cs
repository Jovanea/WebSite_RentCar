using System;
using System.Collections.Generic;
using System.Linq;
using Web.Interfaces;
using eUseControl.BusinessLogic.DBModel;
using eUseControl.Domain.Entities.Car;

namespace Web.BusinessLogic
{
    public class CarApi : ICarApi
    {
        private readonly ApplicationDbContext _context;

        public CarApi()
        {
            _context = new ApplicationDbContext();
        }

        // Funcții de mapare între Car și CarDetails
        private CarDetails ToCarDetails(Car car)
        {
            if (car == null) return null;
            return new CarDetails
            {
                Id = car.CarId,
                Name = car.Brand + " " + car.Model,
                Price = car.PricePerDay,
                ImageUrl = car.MainImageUrl
            };
        }

        private Car ToCar(CarDetails details)
        {
            if (details == null) return null;
            return new Car
            {
                CarId = details.Id,
                Brand = details.Name?.Split(' ').FirstOrDefault() ?? "",
                Model = details.Name?.Split(' ').Skip(1).FirstOrDefault() ?? "",
                PricePerDay = details.Price,
                MainImageUrl = details.ImageUrl,
                Year = 2020,
                Transmission = "Manual",
                FuelType = "Benzina",
                Horsepower = 100,
                Seats = 4,
                Category = "Standard",
                IsAvailable = true
            };
        }

        public CarDetails GetCarById(int id)
        {
            return ToCarDetails(_context.Cars.Find(id));
        }

        public List<CarDetails> GetAllCars()
        {
            return _context.Cars.ToList().Select(ToCarDetails).ToList();
        }

        public bool AddCar(CarDetails carDetails)
        {
            try
            {
                var car = ToCar(carDetails);
                _context.Cars.Add(car);
                _context.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool UpdateCar(CarDetails carDetails)
        {
            try
            {
                var existingCar = _context.Cars.Find(carDetails.Id);
                if (existingCar == null) return false;

                existingCar.Brand = carDetails.Name?.Split(' ').FirstOrDefault() ?? existingCar.Brand;
                existingCar.Model = carDetails.Name?.Split(' ').Skip(1).FirstOrDefault() ?? existingCar.Model;
                existingCar.PricePerDay = carDetails.Price;
                existingCar.MainImageUrl = carDetails.ImageUrl ?? existingCar.MainImageUrl;

                _context.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool DeleteCar(int id)
        {
            try
            {
                var car = _context.Cars.Find(id);
                if (car == null) return false;

                _context.Cars.Remove(car);
                _context.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
