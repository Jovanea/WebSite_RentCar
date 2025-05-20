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
        List<CarDetails> GetAllCars();
        CarDetails GetCarById(int id);
        bool CreateCar(CarDetails car, ImageUpload mainImage, ImageUpload interiorImage, ImageUpload exteriorImage);
        bool UpdateCar(CarDetails car, ImageUpload mainImage, ImageUpload interiorImage, ImageUpload exteriorImage);
        bool DeleteCar(int id);
        bool UpdateUserToAdmin(string email);
        bool CreateAdmin(UserData userData);
    }
}