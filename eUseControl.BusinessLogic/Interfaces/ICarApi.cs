using System;
using System.Collections.Generic;
using eUseControl.Domain.Entities.Car;
using eUseControl.BusinessLogic.DBModel;

namespace Web.Interfaces
{
    public interface ICarApi
    {
        CarDetails GetCarById(int id);
        List<CarDetails> GetAllCars();
        bool AddCar(CarDetails car);
        bool UpdateCar(CarDetails car);
        bool DeleteCar(int id);
    }
}