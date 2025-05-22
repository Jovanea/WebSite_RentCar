using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace eUseControl.BusinessLogic.DBModel
{
    public class Car
    {
        [Key]
        public int CarId { get; set; }

        [Required]
        [StringLength(50)]
        public string Brand { get; set; }

        [Required]
        [StringLength(50)]
        public string Model { get; set; }

        [Required]
        public int Year { get; set; }

        [Required]
        public int PricePerDay { get; set; }

        [Required]
        [StringLength(20)]
        public string Transmission { get; set; }

        [Required]
        [StringLength(20)]
        public string FuelType { get; set; }

        [Required]
        public int Horsepower { get; set; }

        [Required]
        public int Seats { get; set; }

        [Required]
        [StringLength(50)]
        public string Category { get; set; }

        [Required]
        [StringLength(200)]
        public string MainImageUrl { get; set; }

        [StringLength(200)]
        public string InteriorImageUrl { get; set; }

        [StringLength(200)]
        public string ExteriorImageUrl { get; set; }

        [Required]
        public bool IsAvailable { get; set; }

        [Required]
        public int Stock { get; set; } = 1; // Default value of 1

        [StringLength(1000)]
        public string Description { get; set; }

        [StringLength(50)]
        public string Engine { get; set; }

        [StringLength(50)]
        public string Torque { get; set; }

        [StringLength(50)]
        public string Acceleration { get; set; }

        [StringLength(50)]
        public string TopSpeed { get; set; }

        [StringLength(50)]
        public string FuelConsumption { get; set; }

        public virtual ICollection<Booking> Bookings { get; set; }
    }
}