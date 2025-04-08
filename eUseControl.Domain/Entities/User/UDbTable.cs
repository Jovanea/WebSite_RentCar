using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Data.Entity;
using System.Runtime.Remoting.Contexts;
using eUseControl.Domain.Entities.User;

namespace eUseControl.Domain.User.Auth
{
    public class UDbTable
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(30)]
        public string Username { get; set; }

        [Required]
        [StringLength(50)]
        public string Password { get; set; }

        [Required]
        [StringLength(50)]
        public string Email { get; set; }

        [Required]
        public DateTime Last_Login { get; set; }

        [Required]
        [StringLength(30)]
        public string UserIp { get; set; }

        [Required]
        public int Level { get; set; }
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
