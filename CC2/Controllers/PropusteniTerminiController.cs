using CC2.Helpers;
using CC2.Models;
using DataAccess;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CC2.Controllers
{
    public class PropusteniTerminiController : BaseController
    {
        CCEntities efContext = new CCEntities();

        // GET: PropusteniTermini
        [Authorize]
        public ActionResult Index()
        {
            Dashboard list = new Dashboard();
            var datum = DateTime.Now;
            var userId = User.Identity.GetUserId();

            var termini = efContext.TERMINI
                .Where(x => x.USER_ID == userId && x.DATUM <= datum)
                .ToList()
                .Where(ter =>
                    efContext.CC_KONTAKTI.Any(kontakt =>
                        kontakt.ID == ter.KONTAKT_ID &&
                        kontakt.SALES_NIJE_ZAINTERESOVAN != "Y" &&
                        kontakt.PRODAT != "Y"))
                .OrderBy(x => x.DATUM)
                .ToList();

            var terminiWithNames = termini
                .Select(t => new
                {
                    TerminId = t.ID,
                    KontaktIme = efContext.CC_KONTAKTI
                        .Where(k => k.ID == t.KONTAKT_ID)
                        .Select(k => k.IME + " " + k.PREZIME)
                        .FirstOrDefault(),
                    Firma = efContext.CC_KONTAKTI
                        .Where(k => k.ID == t.KONTAKT_ID)
                        .Select(k => k.FIRMA)
                        .FirstOrDefault(),
                    BrojKartica = efContext.CC_KONTAKTI
                        .Where(k => k.ID == t.KONTAKT_ID)
                        .Select(k => k.BROJ_KARTICA)
                        .FirstOrDefault()
                })
                .ToList();

            ViewBag.KontaktImena = terminiWithNames.ToDictionary(x => x.TerminId, x => x.KontaktIme);
            ViewBag.Firme = terminiWithNames.ToDictionary(x => x.TerminId, x => x.Firma);
            ViewBag.BrojKartica = terminiWithNames.ToDictionary(x => x.TerminId, x => x.BrojKartica);

            list.termini = termini;
            return View(list);
        }

    }

}