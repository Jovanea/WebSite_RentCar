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
        [StringLength(13, MinimumLength = 13, ErrorMessage = "Numărul cardului trebuie să aibă 13 cifre")]
        [RegularExpression(@"^[\d\s-]+$", ErrorMessage = "Numărul cardului poate conține doar cifre, spații și cratime")]
        [Display(Name = "Număr card")]
        public string CardNumber { get; set; }

        [Required(ErrorMessage = "CVV-ul este obligatoriu")]
        [StringLength(3, MinimumLength = 3, ErrorMessage = "CVV-ul trebuie să aibă 3 cifre")]
        [RegularExpression(@"^\d+$", ErrorMessage = "CVV-ul poate conține doar cifre")]
        [Display(Name = "CVV")]
        public string CVV { get; set; }

        [Required(ErrorMessage = "Luna expirării este obligatorie")]
        [Display(Name = "Lună expirare")]
        public string ExpiryMonth { get; set; }

        [Required(ErrorMessage = "Anul expirării este obligatoriu")]
        [Display(Name = "An expirare")]
        public string ExpiryYear { get; set; }
    }

}