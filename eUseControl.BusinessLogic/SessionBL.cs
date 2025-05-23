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
using eUseControl.BusinessLogic.Core;
using Web.BusinessLogic;
using eUseControl.BusinessLogic.Interfaces;

namespace eUseControl.BusinessLogic
{
    public class SessionBL : ISession
    {
        private readonly IUserApi _userApi;
        private readonly UserContext _db;

        public SessionBL(IUserApi userApi, UserContext dbContext)
        {
            _userApi = userApi;
            _db = dbContext;
        }

        public UserLogin UserLogin(ULoginData data)
        {
            return _userApi.UserLogin(data);
        }

        public string CreateCookie(ULoginData data)
        {
            if (data == null) return null;

            var userLogin = _userApi.UserLogin(data);

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

                int userId = (int)userData.Id;
                var user = _db.Users.FirstOrDefault(u => u.Id == userId);

                if (user == null) return null;

                return new ULoginData
                {
                    Credential = user.UserName,
                    LoginIp = user.UserIp,
                    LoginDateTime = user.Last_Login
                };
            }
            catch
            {
                return null;
            }
        }
    }
}
