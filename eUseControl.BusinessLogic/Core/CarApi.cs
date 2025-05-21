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
                ImageUrl = car.MainImageUrl,
                Stock = car.Stock
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
                PricePerDay = Convert.ToDecimal(details.Price),
                MainImageUrl = details.ImageUrl,
                Year = 2020,
                Transmission = "Manual",
                FuelType = "Benzina",
                Horsepower = 100,
                Seats = 4,
                Category = "Standard",
                IsAvailable = true,
                Stock = details.Stock
            };
        }

        public CarDetails GetCarById(int id)
        {
            try
            {
                var car = _context.Cars.Find(id);
                if (car == null) return null;
                
                return new CarDetails
                {
                    Id = car.CarId,
                    Name = car.Brand + " " + car.Model,
                    Price = Convert.ToDecimal(car.PricePerDay),
                    ImageUrl = car.MainImageUrl,
                    Stock = car.Stock
                };
            }
            catch (Exception ex)
            {
                // Log the error
                System.Diagnostics.Debug.WriteLine("Error getting car by ID: " + ex.Message);
                return null;
            }
        }

        public List<CarDetails> GetAllCars()
        {
            try
            {
                // Add debugging to check if any cars exist in database
                var carsInDb = _context.Cars.ToList();
                System.Diagnostics.Debug.WriteLine($"Found {carsInDb.Count} cars in database");
                
                foreach (var car in carsInDb)
                {
                    System.Diagnostics.Debug.WriteLine($"Car in DB: ID={car.CarId}, Brand={car.Brand}, Model={car.Model}, Price={car.PricePerDay}, Stock={car.Stock}");
                }
                
                // Explicitly handle the conversion by using a direct SQL query with proper casting
                var cars = new List<CarDetails>();
                foreach (var car in carsInDb)
                {
                    try {
                        cars.Add(new CarDetails
                        {
                            Id = car.CarId,
                            Name = car.Brand + " " + car.Model,
                            Price = Convert.ToDecimal(car.PricePerDay),
                            ImageUrl = car.MainImageUrl,
                            Stock = car.Stock
                        });
                        System.Diagnostics.Debug.WriteLine($"Successfully converted car: {car.Brand} {car.Model}");
                    }
                    catch (Exception ex) {
                        System.Diagnostics.Debug.WriteLine($"Error converting car {car.CarId}: {ex.Message}");
                    }
                }
                System.Diagnostics.Debug.WriteLine($"Returning {cars.Count} car details objects");
                return cars;
            }
            catch (Exception ex)
            {
                // Log the error
                System.Diagnostics.Debug.WriteLine("Error getting cars: " + ex.Message);
                System.Diagnostics.Debug.WriteLine("Stack trace: " + ex.StackTrace);
                return new List<CarDetails>();
            }
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
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error adding car: {ex.Message}");
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
                existingCar.PricePerDay = Convert.ToDecimal(carDetails.Price);
                existingCar.MainImageUrl = carDetails.ImageUrl ?? existingCar.MainImageUrl;
                existingCar.Stock = carDetails.Stock;

                _context.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating car: {ex.Message}");
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
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error deleting car: {ex.Message}");
                return false;
            }
        }
        
        public bool UpdateCarStock(int carId, int stockChange)
        {
            try
            {
                // Retrieve the car from the database
                var car = _context.Cars.Find(carId);
                if (car == null) return false;
                
                // Check if the requested change is valid
                int newStock = car.Stock + stockChange;
                if (newStock < 0)
                {
                    System.Diagnostics.Debug.WriteLine($"Cannot update stock to negative value: {newStock}");
                    return false;
                }
                
                // Update the stock
                car.Stock = newStock;
                
                // If stock becomes 0, set IsAvailable to false
                if (newStock == 0)
                {
                    car.IsAvailable = false;
                }
                else
                {
                    car.IsAvailable = true;
                }
                
                // Save changes to the database
                _context.SaveChanges();
                System.Diagnostics.Debug.WriteLine($"Updated stock for car {carId} to {car.Stock}");
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating stock: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                return false;
            }
        }
        
        public int GetCarStock(int carId)
        {
            try
            {
                var car = _context.Cars.Find(carId);
                if (car == null) return 0;
                
                return car.Stock;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting car stock: {ex.Message}");
                return 0;
            }
        }
    }
}
