using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using eUseControl.Domain.Entities.User;

namespace eUseControl.BusinessLogic
{
    public class SessionBL : UserApi, ISession
    {
        public UserLogin UserLogin(ULoginData data)
        {
            return new UserLogin(data);
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
