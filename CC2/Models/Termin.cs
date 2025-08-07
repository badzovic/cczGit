using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CC2.Models
{
    public class Termin
    {
        public DateTime vrijeme { get; set; }
        public int ID { get; set; }
        public string USER_ID { get; set; }
        public DateTime? DATUM { get; set; }
        public string NAZIV { get; set; }
        public int? KONTAKT_ID { get; set; }
        public string Boja { get; set; }

        public DateTime? KRAJ { get; set; }

        public string UserId { get; set; }
    }
}