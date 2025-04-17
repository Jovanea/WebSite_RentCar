using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace Web.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext() : base("name=WebSite_RentCar")
        {
        }

        public DbSet<Payment> Payments { get; set; }
        public DbSet<Booking> Bookings { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}