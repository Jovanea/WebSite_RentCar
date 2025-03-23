using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using eUseControl.BusinessLogic;
using eUseControl.Domain.Entities.User;
using eUseControl.Domain.Entities.Car;


namespace eUseControl.BusinessLogic
{
    public interface ISession
    {
        UserLogin UserLogin(ULoginData data);
    }

    public interface ICarSession
    {
        CarDetails GetCarDetails(int carId);
        List<CarSection> GetCarSections();
    }


}
namespace eUseControl.BusinessLogic.Interfaces
{
    public interface ISession
    {
        ULoginData UserLogin(ULoginData data);
        ULoginData AdminLogin(ULoginData data); // Noua metodă pentru admin
    }
}


namespace eUseControl.BusinessLogic.Interfaces
{
    public interface ISession
    {
        ULoginData UserLogin(ULoginData data);
        ULoginData AdminLogin(ULoginData data); // Noua metodă pentru admin
    }
}