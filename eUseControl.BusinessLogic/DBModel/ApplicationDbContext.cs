using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace eUseControl.BusinessLogic.DBModel
{
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
            Database.SetInitializer(new ApplicationDbInitializer());
        }

        public DbSet<Booking> Bookings { get; set; }
        public DbSet<Car> Cars { get; set; }
        public DbSet<Payment> Payments { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Booking>()
                .HasOptional(b => b.Payment)
                .WithRequired(p => p.Booking);
            modelBuilder.Entity<Car>()
                .HasMany(c => c.Bookings)
                .WithRequired(b => b.Car)
                .HasForeignKey(b => b.CarId);
        }
    }
}