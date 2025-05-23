using System;
using System.Linq;
using eUseControl.Domain.Entities.User;
using eUseControl.BusinessLogic.DBModel;
using eUseControl.BusinessLogic.Interfaces;
using eUseControl.BusinessLogic.Models;
using eUseControl.Domain.User.Auth;
using eUseControl.BusinessLogic.Core;

namespace eUseControl.BusinessLogic
{
    public class UserApi : IUserApi
    {
        private readonly UserContext _db;

        public UserApi(UserContext dbContext)
        {
            _db = dbContext;
        }

        public UserRegister UserRegister(URegisterData data)
        {
            try
            {
                if (_db.Users.Any(u => u.Email == data.Email))
                {
                    return new UserRegister { Status = false, StatusMsg = "Acest email există deja" };
                }

                if (_db.Users.Any(u => u.UserName == data.UserName))
                {
                    return new UserRegister { Status = false, StatusMsg = "Acest nume de utilizator există deja" };
                }

                string hashedPassword = PasswordHasher.HashPassword(data.Password);

                var newUser = new UDbTable
                {
                    UserName = data.UserName,
                    Password = hashedPassword,
                    Email = data.Email,
                    Phone = data.Phone,
                    Last_Login = DateTime.Now,
                    UserIp = data.UserIp,
                    Level = 0
                };

                _db.Users.Add(newUser);
                int result = _db.SaveChanges();

                if (result > 0)
                {
                    return new UserRegister { Status = true, StatusMsg = "Înregistrare reușită" };
                }
                else
                {
                    return new UserRegister { Status = false, StatusMsg = "Nu s-a putut salva utilizatorul în baza de date" };
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in UserRegister: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                return new UserRegister { Status = false, StatusMsg = "A apărut o eroare la înregistrare: " + ex.Message };
            }
        }

        public UserLogin UserLogin(ULoginData data)
        {
            try
            {
                var user = _db.Users.FirstOrDefault(u =>
                    (u.Email == data.Credential || u.UserName == data.Credential));

                if (user != null && PasswordHasher.VerifyPassword(data.Password, user.Password))
                {
                    user.Last_Login = DateTime.Now;
                    user.UserIp = data.LoginIp;
                    _db.SaveChanges();

                    var userLogin = new UserLogin(data)
                    {
                        Status = true,
                        StatusMsg = "Autentificare reușită!",
                        Credential = user.UserName,
                        Password = data.Password,
                        Phone = user.Phone,
                        Id = user.Id
                    };

                    System.Diagnostics.Debug.WriteLine($"User login successful. Phone: {user.Phone}");
                    return userLogin;
                }

                return new UserLogin(data)
                {
                    Status = false,
                    StatusMsg = "Credențiale incorecte. Vă rugăm să verificați email-ul și parola."
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in UserLogin: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                return new UserLogin(data)
                {
                    Status = false,
                    StatusMsg = "A apărut o eroare la autentificare: " + ex.Message
                };
            }
        }

        public ISession GetSessionBL()
        {
            return new SessionBL(this, _db);
        }
    }
}
