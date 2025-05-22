using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;
using Web.Interfaces;
using eUseControl.BusinessLogic.DBModel;
using eUseControl.BusinessLogic.Models;
using eUseControl.Domain.Entities.Car;
using eUseControl.Domain.User.Auth;

namespace Web.BusinessLogic
{
    public class AdminApi : IAdminApi
    {
        private readonly ApplicationDbContext _context;
        private readonly string _imagePath;
        private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png" };
        private readonly int _maxFileSize = 5 * 1024 * 1024;

        public AdminApi()
        {
            _context = new ApplicationDbContext();
            _imagePath = HttpContext.Current.Server.MapPath("~/Content/images/cars");
        }

        public List<Car> GetAllCars()
        {
            try
            {
                return _context.Cars.ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error getting cars: " + ex.Message);
                return new List<Car>();
            }
        }

        public Car GetCarById(int id)
        {
            try
            {
                return _context.Cars.Find(id);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error getting car by ID: " + ex.Message);
                return null;
            }
        }

        public bool CreateCar(Car car)
        {
            try
            {
                if (!ValidateCarData(car))
                    return false;

                _context.Cars.Add(car);
                _context.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error creating car: " + ex.Message);
                return false;
            }
        }

        public bool UpdateCar(Car car)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"[DEBUG] UpdateCar called for CarId: {car.CarId}");
                var existingCar = _context.Cars.Find(car.CarId);
                if (existingCar == null)
                {
                    System.Diagnostics.Debug.WriteLine($"Car with ID {car.CarId} not found.");
                    return false;
                }

                if (!ValidateCarData(car))
                {
                    System.Diagnostics.Debug.WriteLine("ValidateCarData failed.");
                    return false;
                }

                existingCar.Brand = car.Brand;
                existingCar.Model = car.Model;
                existingCar.Year = car.Year;
                existingCar.PricePerDay = car.PricePerDay;
                existingCar.Transmission = car.Transmission;
                existingCar.FuelType = car.FuelType;
                existingCar.Horsepower = car.Horsepower;
                existingCar.Seats = car.Seats;
                existingCar.Category = car.Category;
                existingCar.Description = car.Description;
                existingCar.Engine = car.Engine;
                existingCar.Torque = car.Torque;
                existingCar.Acceleration = car.Acceleration;
                existingCar.TopSpeed = car.TopSpeed;
                existingCar.FuelConsumption = car.FuelConsumption;
                existingCar.Stock = car.Stock;

                _context.SaveChanges();
                System.Diagnostics.Debug.WriteLine("Changes saved successfully.");
                return true;
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException dbEx)
            {
                var errorMessages = dbEx.EntityValidationErrors
                    .SelectMany(x => x.ValidationErrors)
                    .Select(x => $"Property: {x.PropertyName} Error: {x.ErrorMessage}");
                var fullErrorMessage = string.Join("; ", errorMessages);
                System.Diagnostics.Debug.WriteLine($"DbEntityValidationException in UpdateCar: {fullErrorMessage}");
                return false; 
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating car: {ex.GetType().Name} - {ex.Message}");
                return false;
            }
        }

        public bool DeleteCar(int id)
        {
            try
            {
                var car = _context.Cars.Find(id);
                if (car == null)
                    return false;

                _context.Cars.Remove(car);
                _context.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error deleting car: " + ex.Message);
                return false;
            }
        }

        public bool ToggleCarAvailability(int id)
        {
            try
            {
                var car = _context.Cars.Find(id);
                if (car == null)
                    return false;

                car.IsAvailable = !car.IsAvailable;
                _context.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error toggling car availability: " + ex.Message);
                return false;
            }
        }

        public bool UpdateCarStock(int carId, int stockChange)
        {
            try
            {
                var car = _context.Cars.Find(carId);
                if (car == null)
                    return false;

                car.Stock += stockChange;
                if (car.Stock < 0)
                    car.Stock = 0;

                _context.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error updating car stock: " + ex.Message);
                return false;
            }
        }

        public bool SaveCarImage(int carId, ImageUpload image, string imageType)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"[DEBUG] SaveCarImage called for CarId: {carId}, imageType: {imageType}");

                if (image == null || !ValidateImage(image))
                {
                    System.Diagnostics.Debug.WriteLine("[DEBUG] Image validation failed.");
                    return false;
                }

                var car = _context.Cars.Find(carId);
                if (car == null)
                {
                    System.Diagnostics.Debug.WriteLine($"[DEBUG] Car with ID {carId} not found in SaveCarImage.");
                    return false;
                }

                var fileName = GenerateUniqueFileName(image.FileName);
                var path = Path.Combine(_imagePath, fileName);

                System.Diagnostics.Debug.WriteLine($"[DEBUG] Saving image to path: {path}");
                File.WriteAllBytes(path, image.Content);
                System.Diagnostics.Debug.WriteLine("[DEBUG] Image file written.");

                switch (imageType.ToLower())
                {
                    case "main":
                        car.MainImageUrl = "/Content/images/cars/" + fileName;
                        break;
                    case "interior":
                        car.InteriorImageUrl = "/Content/images/cars/" + fileName;
                        break;
                    case "exterior":
                        car.ExteriorImageUrl = "/Content/images/cars/" + fileName;
                        break;
                    default:
                        System.Diagnostics.Debug.WriteLine($"[DEBUG] Invalid imageType: {imageType}");
                        return false;
                }

                System.Diagnostics.Debug.WriteLine("[DEBUG] Attempting to save image URL to database.");
                _context.SaveChanges();
                System.Diagnostics.Debug.WriteLine("[DEBUG] Image URL saved to database.");

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[DEBUG] Error saving car image: {ex.GetType().Name} - {ex.Message}");
                return false;
            }
        }

        public bool DeleteCarImage(int carId, string imageType)
        {
            try
            {
                var car = _context.Cars.Find(carId);
                if (car == null)
                    return false;

                string imageUrl = null;
                switch (imageType.ToLower())
                {
                    case "main":
                        imageUrl = car.MainImageUrl;
                        car.MainImageUrl = null;
                        break;
                    case "interior":
                        imageUrl = car.InteriorImageUrl;
                        car.InteriorImageUrl = null;
                        break;
                    case "exterior":
                        imageUrl = car.ExteriorImageUrl;
                        car.ExteriorImageUrl = null;
                        break;
                    default:
                        return false;
                }

                if (!string.IsNullOrEmpty(imageUrl))
                {
                    var fileName = Path.GetFileName(imageUrl);
                    var path = Path.Combine(_imagePath, fileName);
                    if (File.Exists(path))
                    {
                        File.Delete(path);
                    }
                }

                _context.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error deleting car image: " + ex.Message);
                return false;
            }
        }

        private bool ValidateCarData(Car car)
        {
            if (string.IsNullOrEmpty(car.Brand) ||
                string.IsNullOrEmpty(car.Model) ||
                car.Year < 1900 ||
                car.Year > DateTime.Now.Year + 1 ||
                car.PricePerDay <= 0 ||
                string.IsNullOrEmpty(car.Transmission) ||
                string.IsNullOrEmpty(car.FuelType) ||
                car.Horsepower <= 0 ||
                car.Seats <= 0 ||
                string.IsNullOrEmpty(car.Category))
                return false;

            return true;
        }

        private bool ValidateImage(ImageUpload image)
        {
            if (image == null || image.Content == null || image.Content.Length == 0)
                return false;

            if (image.Content.Length > _maxFileSize)
                return false;

            var extension = Path.GetExtension(image.FileName).ToLower();
            if (!_allowedExtensions.Contains(extension))
                return false;

            return true;
        }

        private string GenerateUniqueFileName(string originalFileName)
        {
            var extension = Path.GetExtension(originalFileName);
            return Guid.NewGuid().ToString() + extension;
        }

        public bool UpdateUserToAdmin(string email)
        {
            try
            {
                var user = _context.Users.FirstOrDefault(u => u.Email == email);
                if (user == null)
                    return false;

                user.Level = 1;
                _context.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error updating user to admin: " + ex.Message);
                return false;
            }
        }

        public bool CreateAdmin(UserData userData)
        {
            try
            {
                var user = new UDbTable
                {
                    Email = userData.Email,
                    Password = userData.Password,
                    Level = 1
                };

                _context.Users.Add(user);
                _context.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error creating admin: " + ex.Message);
                return false;
            }
        }
    }
}