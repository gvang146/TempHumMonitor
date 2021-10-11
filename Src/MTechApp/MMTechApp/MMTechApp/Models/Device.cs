using System;
using System.Collections.Generic;
using System.Text;

namespace MMTechApp.Models
{ // Device for people to work on
    public class Device
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
