using System;
using System.ComponentModel.DataAnnotations;

namespace eUseControl.Domain.Entities.User
{
    public class URegisterData
    {
        [Required(ErrorMessage = "Numele și prenumele sunt obligatorii")]
        [StringLength(30, MinimumLength = 5, ErrorMessage = "Numele trebuie să aibă între 5 și 30 de caractere")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Email-ul este obligatoriu")]
        [EmailAddress(ErrorMessage = "Introduceți un email valid")]
        [StringLength(50)]
        public string Email { get; set; }

        [Required(ErrorMessage = "Parola este obligatorie")]
        [StringLength(50, MinimumLength = 8, ErrorMessage = "Parola trebuie să aibă cel puțin 8 caractere")]
        public string Password { get; set; }

        public string UserIp { get; set; }
        public DateTime LastLogin { get; set; }
        public int Level { get; set; }
    }
}