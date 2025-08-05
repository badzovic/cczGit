using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CC2.Models
{
    public class Kontakti
    {
        public int Id { get; set; }    
        public string gender { get; set; }
        public string ime { get; set; }
        public string prezime { get; set; }
        public string adresa { get; set; }
        public string plz { get; set; }
        public string drzava { get; set; }
        public string grad { get; set; }
        public string prefix { get; set; }
        public string broj { get; set; }
        public string broj2 { get; set; }
        public string email { get; set; }
        public string firma { get; set; }
        public string brojkartica { get; set; }
        public string fax { get; set; }
        public string bei { get; set; }
        public string dostupnost { get; set; }
        public string investicija { get; set; }
        public DateTime terminDate { get; set; }
        public string komentar { get; set; }
        public string buttonZainteresovan { get; set; }
        public string buttonSacuvaj { get; set; }
        public string submitButtonHidden { get; set; }
        public int terminId { get; set;}
        public string vracenMarketingu { get; set; }
        public string vracenSakontrole { get; set; }
        public List<Termin> Termini { get; set; }
        

    }
}