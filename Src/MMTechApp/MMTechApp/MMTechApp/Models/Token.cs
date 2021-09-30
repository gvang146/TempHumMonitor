using System;
using System.Collections.Generic;
using System.Text;

namespace MMTechApp.Models
{
    class Token
    {
        public int Id { get; set; }
        public string accessToken { get; set; }
        public string errDescription { get; set; }
        public DateTime exDate { get; set; }

        public Token()
        {

        }
    }
}
