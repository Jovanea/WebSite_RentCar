using System;
using System.Collections.Generic;
using System.Web;
using eUseControl.Domain.Entities.Car;
using eUseControl.BusinessLogic.DBModel;
using eUseControl.BusinessLogic.Models;

namespace Web.Interfaces
{
    public interface IAdminApi
    {
        List<Car> GetAllCars();
        Car GetCarById(int id);
        bool CreateCar(Car car);
        bool UpdateCar(Car car);
        bool DeleteCar(int id);
        bool ToggleCarAvailability(int id);
        bool UpdateCarStock(int carId, int stockChange);

        // Image Management
        bool SaveCarImage(int carId, ImageUpload image, string imageType);
        bool DeleteCarImage(int carId, string imageType);

        // Admin Management
        bool UpdateUserToAdmin(string email);
        bool CreateAdmin(UserData userData);
    }
}