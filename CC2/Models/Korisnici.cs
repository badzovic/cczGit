using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CC2.Models;

namespace CC2.Models
{
    public class Korisnici
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string UserRole { get; set; }
        public List<string> Roles { get; set; }
    }
}