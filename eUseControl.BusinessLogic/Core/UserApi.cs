using System;
using System.Linq;
using eUseControl.Domain.Entities.User;
using eUseControl.BusinessLogic.Core.DBModel;
using eUseControl.BusinessLogic.Interfaces;
using eUseControl.BusinessLogic.Models;
using eUseControl.Domain.User.Auth;

namespace eUseControl.BusinessLogic
{
    public class UserApi : IUserApi
    {
        public UserRegister UserRegister(URegisterData data)
        {
            try
            {
                using (var db = new UserContext())
                {
                    // Check if username or email already exists
                    if (db.Users.Any(u => u.Email == data.Email))
                    {
                        return new UserRegister { Status = false, StatusMsg = "Acest email există deja" };
                    }

                    if (db.Users.Any(u => u.UserName == data.UserName))
                    {
                        return new UserRegister { Status = false, StatusMsg = "Acest nume de utilizator există deja" };
                    }

                    // Create new user
                    var newUser = new UDbTable
                    {
                        UserName = data.UserName,
                        Password = data.Password,
                        Email = data.Email,
                        Phone = data.Phone,
                        Last_Login = DateTime.Now,
                        UserIp = data.UserIp,
                        Level = 0 // Default level for new users
                    };

                    db.Users.Add(newUser);
                    int result = db.SaveChanges();

                    if (result > 0)
                    {
                        return new UserRegister { Status = true, StatusMsg = "Înregistrare reușită" };
                    }
                    else
                    {
                        return new UserRegister { Status = false, StatusMsg = "Nu s-a putut salva utilizatorul în baza de date" };
                    }
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
                using (var db = new UserContext())
                {
                    // Check if user exists with the given credentials (email and password)
                    var user = db.Users.FirstOrDefault(u =>
                    (u.Email == data.Credential || u.UserName == data.Credential) &&
                     u.Password == data.Password);

                    if (user != null)
                    {
                        // Update last login time and IP
                        user.Last_Login = DateTime.Now;
                        user.UserIp = data.LoginIp;
                        db.SaveChanges();

                        // Create a new UserLogin object with all user information
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
    }
}
