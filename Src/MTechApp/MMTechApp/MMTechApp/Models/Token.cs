using System;
using System.Collections.Generic;
using System.Text;

namespace MMTechApp.Models
{   // token to acccess the data stored
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
