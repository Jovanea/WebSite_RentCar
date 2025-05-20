using System.Data.Entity;
using eUseControl.Domain.User.Auth;


namespace eUseControl.BusinessLogic.DBModel
{
    public class UserContext : DbContext
    {
        public UserContext() : base("name=WebSite_RentCar") { }

        public virtual DbSet<UDbTable> Users { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UDbTable>()
                .Property(e => e.UserName)
                .IsUnicode(false);

            modelBuilder.Entity<UDbTable>()
                .Property(e => e.Password)
                .IsUnicode(false);

            modelBuilder.Entity<UDbTable>()
                .Property(e => e.Email)
                .IsUnicode(false);

            modelBuilder.Entity<UDbTable>()
                .Property(e => e.UserIp)
                .IsUnicode(false);
        }
    }
}