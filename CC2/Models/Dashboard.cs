using DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CC2.Models
{
    public class Dashboard
    {
        public List<TERMINI> termini { get; set; }
        public List<CC_KONTAKTI> uPregovorima { get; set; }
        public List<CC_KONTAKTI> prodati { get; set; }
        public List<CC_KONTAKTI> nisuZainteresovani { get; set; }
        public List<CC_KONTAKTI> uPregovorimaUser { get; set; }
        public List<CC_KONTAKTI> prodatiUser { get; set; }
        public List<CC_KONTAKTI> nisuZainteresovaniUser { get; set; }
        public int zakazanihTermina { get; set; }
        public int ukupnoKlijenti { get; set; }
        public int ukupnoVraceniKlijenti { get; set; }
        public string graphData { get; set; }


    }
}