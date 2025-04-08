using System.Collections.Generic;
using System.Data.Entity;
using System.Runtime.Remoting.Contexts;
using eUseControl.Domain.Entities.User;

namespace eUseControl.Domain.User
{
    class UDbTable
    {

    }

    class UserContext : DbContext
    {
        public UserContext() :
            base("name=eUseControl")
        {
        }

        public virtual DbSet<UDbTable> Users { get; set; }
    }
}
