using System.Collections.Generic;

namespace eUseControl.BusinessLogic.Models
{
    public class UserData
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Phone { get; set; }
        public List<string> Products { get; set; }

    }
}