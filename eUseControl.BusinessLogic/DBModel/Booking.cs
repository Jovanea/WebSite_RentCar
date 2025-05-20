using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace eUseControl.BusinessLogic.DBModel
{
    public class Booking
    {
        [Key]
        public int BookingId { get; set; }

        [Required]
        public int CarId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public DateTime PickupDate { get; set; }

        [Required]
        public DateTime ReturnDate { get; set; }

        [Required]
        public decimal TotalAmount { get; set; }

        [Required]
        public string Status { get; set; }
        [ForeignKey("CarId")]
        public virtual Car Car { get; set; }

        public virtual Payment Payment { get; set; }
    }
}