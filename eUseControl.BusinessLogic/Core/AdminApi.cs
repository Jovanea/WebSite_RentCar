using System;
using System.Collections.Generic;
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
        private readonly int _maxFileSize = 5 * 1024 * 1024; // 5MB

        public AdminApi()
        {
            _context = new ApplicationDbContext();
            _imagePath = HttpContext.Current.Server.MapPath("~/Content/images/cars");
        }

        public List<CarDetails> GetAllCars()
        {
            try
            {
                // Explicitly handle the conversion by using a manual conversion
                var cars = new List<CarDetails>();
                foreach (var car in _context.Cars.ToList())
                {
                    cars.Add(new CarDetails
                    {
                        Id = car.CarId,
                        Name = car.Brand + " " + car.Model,
                        Price = Convert.ToDecimal(car.PricePerDay),
                        ImageUrl = car.MainImageUrl
                    });
                }
                return cars;
            }
            catch (Exception ex)
            {
                // Log the error
                System.Diagnostics.Debug.WriteLine("Error getting cars: " + ex.Message);
                return new List<CarDetails>();
            }
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
                    ImageUrl = car.MainImageUrl
                };
            }
            catch (Exception ex)
            {
                // Log the error
                System.Diagnostics.Debug.WriteLine("Error getting car by ID: " + ex.Message);
                return null;
            }
        }

        public bool CreateCar(CarDetails carDetails, ImageUpload mainImage, ImageUpload interiorImage, ImageUpload exteriorImage)
        {
            try
            {
                var car = ToCar(carDetails);
                if (!ValidateCarData(car))
                    return false;

                if (mainImage != null && !ValidateImage(mainImage))
                    return false;

                if (mainImage != null && mainImage.Content != null && mainImage.Content.Length > 0)
                {
                    var fileName = GenerateUniqueFileName(mainImage.FileName);
                    var path = Path.Combine(_imagePath, fileName);
                    File.WriteAllBytes(path, mainImage.Content);
                    car.MainImageUrl = "/Content/images/cars/" + fileName;
                }

                _context.Cars.Add(car);
                _context.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool UpdateCar(CarDetails carDetails, ImageUpload mainImage, ImageUpload interiorImage, ImageUpload exteriorImage)
        {
            try
            {
                var existingCar = _context.Cars.Find(carDetails.Id);
                if (existingCar == null) return false;

                if (!ValidateCarData(existingCar))
                    return false;

                if (mainImage != null && !ValidateImage(mainImage))
                    return false;

                if (mainImage != null && mainImage.Content != null && mainImage.Content.Length > 0)
                {
                    DeleteExistingImage(existingCar.MainImageUrl);
                    var fileName = GenerateUniqueFileName(mainImage.FileName);
                    var path = Path.Combine(_imagePath, fileName);
                    File.WriteAllBytes(path, mainImage.Content);
                    existingCar.MainImageUrl = "/Content/images/cars/" + fileName;
                }

                existingCar.Brand = carDetails.Name?.Split(' ').FirstOrDefault() ?? existingCar.Brand;
                existingCar.Model = carDetails.Name?.Split(' ').Skip(1).FirstOrDefault() ?? existingCar.Model;
                existingCar.PricePerDay = Convert.ToDecimal(carDetails.Price);
                existingCar.MainImageUrl = carDetails.ImageUrl ?? existingCar.MainImageUrl;

                _context.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }

        private bool ValidateCarData(Car car)
        {
            if (string.IsNullOrEmpty(car.Brand) ||
                string.IsNullOrEmpty(car.Model) ||
                car.Year < 1900 ||
                car.Year > DateTime.Now.Year + 1 ||
                car.PricePerDay <= 0)
                return false;

            return true;
        }

        private bool ValidateImage(ImageUpload image)
        {
            if (image == null || image.Content == null || image.Content.Length == 0)
                return true;

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

        private void DeleteExistingImage(string imageUrl)
        {
            if (!string.IsNullOrEmpty(imageUrl))
            {
                var imagePath = Path.Combine(_imagePath, Path.GetFileName(imageUrl));
                if (File.Exists(imagePath))
                    File.Delete(imagePath);
            }
        }

        public bool DeleteCar(int id)
        {
            try
            {
                var car = _context.Cars.Find(id);
                if (car == null) return false;

                DeleteExistingImage(car.MainImageUrl);

                _context.Cars.Remove(car);
                _context.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool UpdateUserToAdmin(string email)
        {
            try
            {
                var user = _context.Users.FirstOrDefault(u => u.Email == email);
                if (user == null) return false;

                user.Level = 1;
                _context.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool CreateAdmin(UserData userData)
        {
            try
            {
                var user = new UDbTable
                {
                    UserName = userData.Username,
                    Email = string.IsNullOrEmpty(userData.Email) ? userData.Username + "@example.com" : userData.Email,
                    Password = string.IsNullOrEmpty(userData.Password) ? "parola1234" : userData.Password,
                    Phone = string.IsNullOrEmpty(userData.Phone) ? "000000000" : userData.Phone,
                    UserIp = "127.0.0.1",
                    Last_Login = DateTime.Now,
                    Level = 1
                };
                _context.Users.Add(user);
                _context.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }

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
                PricePerDay = Convert.ToDecimal(details.Price),
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
    }
}