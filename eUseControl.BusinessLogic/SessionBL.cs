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
    public class SessionBL : UserApi, ISession
    {
        public UserLogin UserLogin(ULoginData data)
        {
            return new UserLogin(data);
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

// logina as admin

//namespace eUseControl.BusinessLogic
//{
//    public class SessionBL1 : ISession
//    {
//        public ULoginData UserLogin(ULoginData data)
//        {
//            // Deocamdată returnează o instanță goală pentru a evita eroarea
//            return new ULoginData
//            {
//                Status = false,
//                StatusMsg = "User login not implemented"
//            };
//        }

//        public ULoginData AdminLogin(ULoginData data)
//        {
//            if (data.Credential == "admin" && data.Password == "admin123")
//            {
//                return new ULoginData
//                {
//                    Status = true,
//                    StatusMsg = "Login successful"
//                };
//            }

//            return new ULoginData
//            {
//                Status = false,
//                StatusMsg = "Invalid admin credentials"
//            };
//        }
//    }
//}
