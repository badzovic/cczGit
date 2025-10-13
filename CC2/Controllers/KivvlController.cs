using CC2.Models;
using DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace CC2.Controllers
{
    [Authorize]
    public class KivvlController : Controller
    {
        [HttpGet]
        public ActionResult Index()
        {
            using (var efContext = new CCEntities())
            {
                Pregled pregled = new Pregled();

                // Prikaz svih kontakata koji imaju KIVVL = "DA"
                var kontaktiKiVvl = efContext.CC_KONTAKTI
                    .Where(k =>
                        k.KIVVL == "DA" &&
                        k.PRODAT != "Y" &&
                        k.NIJE_DOBIJEN != "Y")
                    .OrderByDescending(k => k.DATETIME_UPDATED)
                    .ToList();

                // Direktno dodaj u listu bez termina
                pregled.kontaktiSales = kontaktiKiVvl
                    .Select(k => new KontaktSaTerminima
                    {
                        Kontakt = k,
                        SljedeciTermin = null,
                        SljedeciTerminId = null
                    })
                    .ToList();

                ViewBag.Title = "Pregled KI VVL kontakata";

                return View(pregled);
            }
        }
    }
}
