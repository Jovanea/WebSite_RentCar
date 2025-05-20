namespace eUseControl.BusinessLogic.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Bookings",
                c => new
                    {
                        BookingId = c.Int(nullable: false, identity: true),
                        CarId = c.Int(nullable: false),
                        UserId = c.Int(nullable: false),
                        PickupDate = c.DateTime(nullable: false),
                        ReturnDate = c.DateTime(nullable: false),
                        TotalAmount = c.Int(nullable: false),
                        Status = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.BookingId)
                .ForeignKey("dbo.Cars", t => t.CarId, cascadeDelete: true)
                .Index(t => t.CarId);
            
            CreateTable(
                "dbo.Cars",
                c => new
                    {
                        CarId = c.Int(nullable: false, identity: true),
                        Brand = c.String(nullable: false, maxLength: 50),
                        Model = c.String(nullable: false, maxLength: 50),
                        Year = c.Int(nullable: false),
                        PricePerDay = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Transmission = c.String(nullable: false, maxLength: 20),
                        FuelType = c.String(nullable: false, maxLength: 20),
                        Horsepower = c.Int(nullable: false),
                        Seats = c.Int(nullable: false),
                        Category = c.String(nullable: false, maxLength: 50),
                        MainImageUrl = c.String(nullable: false, maxLength: 200),
                        InteriorImageUrl = c.String(maxLength: 200),
                        ExteriorImageUrl = c.String(maxLength: 200),
                        IsAvailable = c.Boolean(nullable: false),
                        Description = c.String(maxLength: 1000),
                        Engine = c.String(maxLength: 50),
                        Torque = c.String(maxLength: 50),
                        Acceleration = c.String(maxLength: 50),
                        TopSpeed = c.String(maxLength: 50),
                        FuelConsumption = c.String(maxLength: 50),
                    })
                .PrimaryKey(t => t.CarId);
            
            CreateTable(
                "dbo.Payments",
                c => new
                    {
                        PaymentId = c.Int(nullable: false, identity: true),
                        BookingId = c.Int(nullable: false),
                        Amount = c.Int(nullable: false),
                        PaymentDate = c.DateTime(nullable: false),
                        PaymentStatus = c.String(nullable: false),
                        TransactionId = c.String(),
                        CardNumber = c.String(),
                        Status = c.String(),
                    })
                .PrimaryKey(t => t.PaymentId)
                .ForeignKey("dbo.Bookings", t => t.PaymentId)
                .Index(t => t.PaymentId);
            
            CreateTable(
                "dbo.UDbTables",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserName = c.String(nullable: false, maxLength: 30),
                        Email = c.String(nullable: false, maxLength: 50),
                        Password = c.String(nullable: false, maxLength: 50),
                        Phone = c.String(maxLength: 9),
                        UserIp = c.String(maxLength: 50),
                        Last_Login = c.DateTime(nullable: false),
                        Level = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Payments", "PaymentId", "dbo.Bookings");
            DropForeignKey("dbo.Bookings", "CarId", "dbo.Cars");
            DropIndex("dbo.Payments", new[] { "PaymentId" });
            DropIndex("dbo.Bookings", new[] { "CarId" });
            DropTable("dbo.UDbTables");
            DropTable("dbo.Payments");
            DropTable("dbo.Cars");
            DropTable("dbo.Bookings");
        }
    }
}
