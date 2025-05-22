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
        ULoginData GetUserByCookie(string cookieValue);
        string CreateCookie(ULoginData data);
    }

    public interface ICarSession
    {
        CarDetails GetCarDetails(int carId);
    }


}

