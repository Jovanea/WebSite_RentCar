using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eUseControl.Domain.Entities.User
{
    public class UserLogin
    {
        public bool Status { get; private set;}
        public string StatusMsg { get; private set;}
        public string Credential { get; set; }
        public string Password { get; set; }
        public UserLogin() { }
        public UserLogin(ULoginData data)
        {
            if (data == null)
            {
                Status = false;
                StatusMsg = "Datele de autentificare sunt invalide.";
                return;
            }

            // Aici trebuie să adaugi logica de verificare a credențialelor.
            if (data.Credential == "admin" && data.Password == "password") // Exemplu simplu
            {
                Status = true;
                StatusMsg = "Autentificare reusita!";
            }
            else
            {
                Status = false;
                StatusMsg = "Credentiale incorecte.";
            }
        }
    }
}
