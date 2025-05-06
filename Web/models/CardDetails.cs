using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace Web.Models
{
    public class CardDetails
    {
        [Required(ErrorMessage = "Numele deținătorului cardului este obligatoriu")]
        [StringLength(100, ErrorMessage = "Numele deținătorului cardului nu poate depăși 100 de caractere")]
        [Display(Name = "Nume deținător card")]
        public string CardHolderName { get; set; }

        [Required(ErrorMessage = "Numărul cardului este obligatoriu")]
        
        [Display(Name = "Număr card")]
        public string CardNumber { get; set; }

        [Required(ErrorMessage = "CVV-ul este obligatoriu")]
        [Display(Name = "CVV")]
        public string CVV { get; set; }

        [Required(ErrorMessage = "Luna expirării este obligatorie")]
        [RegularExpression(@"^(0[1-9]|1[0-2])$", ErrorMessage = "Luna expirării trebuie să fie între 01 și 12")]
        [Display(Name = "Lună expirare")]
        public string ExpiryMonth { get; set; }

        [Required(ErrorMessage = "Anul expirării este obligatoriu")]
        [RegularExpression(@"^\d{2}$", ErrorMessage = "Anul expirării trebuie să fie format din 2 cifre")]
        [Display(Name = "An expirare")]
        public string ExpiryYear { get; set; }

        public int BookingId { get; set; }

        public bool IsExpiryDateValid()
        {
            if (string.IsNullOrEmpty(ExpiryMonth) || string.IsNullOrEmpty(ExpiryYear))
                return false;

            int currentYear = DateTime.Now.Year % 100; 
            int currentMonth = DateTime.Now.Month;
            int expiryYear = int.Parse(ExpiryYear);
            int expiryMonth = int.Parse(ExpiryMonth);

            if (expiryYear < currentYear)
                return false;

            if (expiryYear == currentYear && expiryMonth < currentMonth)
                return false;

            return true;
        }

        public bool IsNumberValid()
        {
            if (string.IsNullOrEmpty(CardNumber) || CardNumber.Replace(" ", "").Length != 16)
                return false;

            return true;
        }
    }
}