namespace eUseControl.BusinessLogic.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ModifyPricePerDayToInt : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Cars", "PricePerDay", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Cars", "PricePerDay", c => c.Decimal(nullable: false, precision: 18, scale: 2));
        }
    }
}
