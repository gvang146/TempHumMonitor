using System;
using System.Collections.Generic;
using System.Text;

namespace MMTechApp.Models
{ // user information 
     public class User
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Password { get; set; }

        public User() { }
        public User(string Username, string Password)
        {
            Id = Guid.Empty;
            this.Name = Username;
            this.Password = Password;
        }
        public bool CheckInformation()
        {
            if (!this.Name.Equals("") && !this.Password.Equals(""))
                return true;
            else
                return false;
        }
    }
}
