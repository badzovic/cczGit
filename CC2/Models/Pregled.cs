using DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CC2.Models
{
    public class Pregled
    {
        public List<CC_KONTAKTI> kontaktiMarketing { get; set; } = new List<CC_KONTAKTI>();

        public List<CC_KONTAKTI> kontaktiMarketingMoji { get; set; } = new List<CC_KONTAKTI>();

        public List<CC_KONTAKTI> kontaktiAdminMarketing { get; set; } = new List<CC_KONTAKTI>();

        public List<CC_KONTAKTI> kontaktiSalesAdmin { get; set; } = new List<CC_KONTAKTI>();

        public List<CC_KONTAKTI> kontaktiSalesAdminSvi { get; set; } = new List<CC_KONTAKTI>();

        public List<CC_KONTAKTI> kontaktiSales { get; set; } = new List<CC_KONTAKTI>();

        public List<CC_KONTAKTI> kontrola { get; set; } = new List<CC_KONTAKTI>();

        public List<string> salesAgenti { get; set; } = new List<string>();

        public List<CC_KONTAKTI> prodato { get; set; } = new List<CC_KONTAKTI>();

        public List<CC_KONTAKTI> nijeDobijen { get; set; } = new List<CC_KONTAKTI>();

        public List<CC_KONTAKTI> nijeZainteresovanSales { get; set; } = new List<CC_KONTAKTI>();

        public List<CC_KONTAKTI> kontaktiSalesNijeZaintresovan { get; set; } = new List<CC_KONTAKTI>();
        public string SelectedEmail { get; set; }      
        public List<AspNetUsers> sviKorisnici { get; set; }
        public string filter { get; set; }
        public int SelectedId { get; set; }
        public List<UserInfo> Users { get; set; }
        public int SelectedAgentId { get; set; }

        public bool IsNew { get; set; }


    }
}