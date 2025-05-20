using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using eUseControl.BusinessLogic;
using eUseControl.Domain.Entities.User;
using eUseControl.Domain.Entities.Car;
using eUseControl.Helpers;
using Newtonsoft.Json;
using eUseControl.BusinessLogic.DBModel;

namespace eUseControl.BusinessLogic
{
    public class SessionBL : UserApi, ISession
    {
        public new UserLogin UserLogin(ULoginData data)
        {
            return new UserLogin(data);
        }

        public string CreateCookie(ULoginData data)
        {
            if (data == null) return null;

            var userApi = new UserApi();
            var userLogin = userApi.UserLogin(data);

            if (!userLogin.Status) return null;

            var userData = new
            {
                Id = userLogin.Id,
                UserName = userLogin.Credential,
                Email = data.Credential,
                Phone = userLogin.Phone,
                LoginDateTime = DateTime.Now
            };

            string json = JsonConvert.SerializeObject(userData);
            return CookieGenerator.Create(json);
        }

        public ULoginData GetUserByCookie(string cookieValue)
        {
            if (string.IsNullOrEmpty(cookieValue)) return null;

            try
            {
                string json = CookieGenerator.Validate(cookieValue);
                if (string.IsNullOrEmpty(json)) return null;

                dynamic userData = JsonConvert.DeserializeObject(json);
                if (userData == null) return null;

                using (var db = new UserContext())
                {
                    int userId = (int)userData.Id;
                    var user = db.Users.FirstOrDefault(u => u.Id == userId);
                    if (user == null) return null;

                    return new ULoginData
                    {
                        Credential = user.UserName,
                        LoginIp = user.UserIp,
                        LoginDateTime = user.Last_Login
                    };
                }
            }
            catch
            {
                return null;
            }
        }
    }

    public class CarSessionBL : CarApi, ICarSession
    {
        public CarDetails GetCarDetails(int carId)
        {
            return new CarDetails
            {
                Id = carId,
                Name = "BMW",
                Price = 850,
                ImageUrl = "/Content/images/cars_pexels_11.jpg"

            };
        }

        public List<CarSection> GetCarSections()
        {
            return new List<CarSection>
            {
                new CarSection
                {
                    Id = 1,
                    Name = "SUV",
                },
                new CarSection
                {
                    Id = 2,
                    Name = "SUV",
                }
            };
        }
    }
}
