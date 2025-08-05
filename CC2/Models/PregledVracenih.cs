using DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CC2.Models
{
    public class PregledVracenih
    {
        public List<CC_KONTAKTI> vraceniKontaktiMarketing { get; set; } = new List<CC_KONTAKTI>();
    }
}