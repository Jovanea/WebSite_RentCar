using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace Web.Models
{
    // Clasa de inițializare a bazei de date
    public class ApplicationDbInitializer : CreateDatabaseIfNotExists<ApplicationDbContext>
    {
        protected override void Seed(ApplicationDbContext context)
        {
            base.Seed(context);
        }
    }

    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext() : base("name=WebSite_RentCar")
        {
            // Setăm inițializatorul bazei de date
            Database.SetInitializer(new ApplicationDbInitializer());
        }

        public DbSet<Booking> Bookings { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configurarea relației între Booking și Payment (1-la-1)
            modelBuilder.Entity<Booking>()
                .HasOptional(b => b.Payment)
                .WithRequired(p => p.Booking);
        }
    }
}