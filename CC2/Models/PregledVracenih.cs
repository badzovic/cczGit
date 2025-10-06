using CC2.Controllers;
using DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using static CC2.Controllers.PregledVracenihController;

namespace CC2.Models
{
    public class PregledVracenih
    {
        public List<CC_KONTAKTI> vraceniKontaktiMarketing { get; set; } = new List<CC_KONTAKTI>();

        public List<KontaktSaTerminima> kontaktiMarketing { get; set; } = new List<KontaktSaTerminima>();

        public List<CC_KONTAKTI> kontaktiMarketingMoji { get; set; } = new List<CC_KONTAKTI>();

        public List<KontaktSaTerminima> kontaktiAdminMarketing { get; set; } = new List<KontaktSaTerminima>();

        public List<KontaktSaTerminima> kontaktiSalesAdmin { get; set; } = new List<KontaktSaTerminima>();

        public List<KontaktSaTerminima> kontaktiSalesAdminSvi { get; set; } = new List<KontaktSaTerminima>();

        public List<KontaktSaTerminima> kontaktiSales { get; set; } = new List<KontaktSaTerminima>();

        public List<KontaktSaTerminima> kontrola { get; set; } = new List<KontaktSaTerminima>();

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