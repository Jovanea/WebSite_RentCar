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
            System.Diagnostics.Debug.WriteLine("Database seeding started");
            try
            {
                // Check if there are already cars in the database
                if (!context.Cars.Any())
                {
                    System.Diagnostics.Debug.WriteLine("No cars found, seeding sample data");
                    
                    // Add sample cars
                    var cars = new List<Car>
                    {
                        new Car { Brand = "Range Rover", Model = "Range Sport", Year = 2020, PricePerDay = 60m, Transmission = "Automată", FuelType = "Diesel", Horsepower = 340, Seats = 5, Category = "SUV", MainImageUrl = "/Content/images/cars_pexels_101.jpg", IsAvailable = true },
                        new Car { Brand = "Mercedes", Model = "Maybach 600", Year = 2018, PricePerDay = 100m, Transmission = "Automată", FuelType = "Benzină", Horsepower = 630, Seats = 4, Category = "Luxury", MainImageUrl = "/Content/images/cars_pexels_2.jpg", IsAvailable = true }
                    };
                    
                    cars.ForEach(c => context.Cars.Add(c));
                    context.SaveChanges();
                    System.Diagnostics.Debug.WriteLine($"Added {cars.Count} sample cars");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"Found {context.Cars.Count()} existing cars, skipping seed");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in seed method: {ex.Message}");
            }
            
            base.Seed(context);
        }
    }

    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext() : base("name=WebSite_RentCar")
        {
            System.Diagnostics.Debug.WriteLine("ApplicationDbContext constructor called");
            Database.SetInitializer(new ApplicationDbInitializer());
            
            // Force initialization
            try
            {
                Database.Initialize(force: false);
                System.Diagnostics.Debug.WriteLine("Database initialized successfully");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Database initialization error: {ex.Message}");
            }
        }

        public DbSet<Booking> Bookings { get; set; }
        public DbSet<Car> Cars { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<eUseControl.Domain.User.Auth.UDbTable> Users { get; set; }

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
            modelBuilder.Entity<Car>()
                .Property(c => c.PricePerDay)
                .HasPrecision(18, 2);
        }
    }
}