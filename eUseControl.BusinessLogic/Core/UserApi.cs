using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using eUseControl.Domain.Entities.User;
using eUseControl.Domain.User.Auth;
using eUseControl.BusinessLogic.Core.DBModel;
using eUseControl.BusinessLogic.Interfaces;
using eUseControl.BusinessLogic.Models;

namespace eUseControl.BusinessLogic
{
    public class UserApi : IUserApi
    {
        public UserRegister UserRegister(URegisterData data)
        {
            using (var db = new UserContext())
            {
                // Check if username or email already exists
                if (db.Users.Any(u => u.Username == data.Username))
                {
                    return new UserRegister { Status = false, StatusMsg = "Acest nume de utilizator există deja" };
                }

                if (db.Users.Any(u => u.Email == data.Email))
                {
                    return new UserRegister { Status = false, StatusMsg = "Acest email există deja" };
                }

                // Create new user
                var newUser = new UDbTable
                {
                    Username = data.Username,
                    Password = data.Password, // In production, you should hash the password
                    Email = data.Email,
                    Last_Login = DateTime.Now,
                    UserIp = data.UserIp,
                    Level = 0 // Default level for new users
                };

                db.Users.Add(newUser);
                db.SaveChanges();

                return new UserRegister { Status = true, StatusMsg = "Înregistrare reușită" };
            }
        }
    }
}
