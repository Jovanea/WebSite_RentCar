using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eUseControl.Domain.Entities.User
{
    public class UserLogin
    {
        public bool Status { get; set; }
        public string StatusMsg { get; set; }
        public string Credential { get; set; }
        public string Password { get; set; }
        public string Phone { get; set; }
        public int Id { get; set; }

        public UserLogin() { }

        public UserLogin(ULoginData data)
        {
            if (data == null)
            {
                Status = false;
                StatusMsg = "Datele de autentificare sunt invalide.";
                return;
            }

            // Constructor gol - logica de autentificare este în UserApi
            Status = false;
            StatusMsg = "Necesită autentificare.";
            Credential = data.Credential;
            Password = data.Password;
        }
    }
}
