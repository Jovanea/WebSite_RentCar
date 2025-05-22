namespace eUseControl.BusinessLogic.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateCarModelWithStock : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Cars", "Stock", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Cars", "Stock");
        }
    }
}
