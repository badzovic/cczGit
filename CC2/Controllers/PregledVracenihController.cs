using CC2.Models;
using DataAccess;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using NLog;

namespace CC2.Controllers
{
    public class PregledVracenihController : Controller
    {
        CCEntities efContext = new CCEntities();
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        // GET: PregledVracenih
        [Authorize]
        public ActionResult Index()
        {
            PregledVracenih pregled = new PregledVracenih();
            var user = HttpContext.User;
            var username = user.Identity.Name;
            var userId = User.Identity.GetUserId();

            if (user.IsInRole("marketing"))
            {
                pregled.vraceniKontaktiMarketing = efContext.CC_KONTAKTI
               .Where(k => k.TRENUTNO_KOD_ID == userId && k.TRENUTNO_GRUPA_ID == "2" && k.VRACEN_MARKETINGU == "Y").OrderByDescending(k => k.ID)
               .ToList();
            }
            else if (user.IsInRole("adminmarketing"))
            {
                pregled.vraceniKontaktiMarketing = efContext.CC_KONTAKTI
               .Where(k => k.TRENUTNO_GRUPA_ID == "4" && k.VRACENO_SA_KONTROLE == "Y").OrderByDescending(k => k.ID)
               .ToList();
            }
            return View(pregled);
        }
        [Authorize]
        public ActionResult Prosljedi(int id)
        {

            var kontakt = efContext.CC_KONTAKTI.Find(id);

            using (var transaction = efContext.Database.BeginTransaction())
            {
                try
                {
                    kontakt.TRENUTNO_GRUPA_ID = "4";

                    int results2 = efContext.SaveChanges();
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    _logger.Error("Greska na metodi Prosljedi get." + " " + ex.Message);
                }
            }

            TempData["Success"] = "Uspješno ste prosljedili kontakt!";

            return RedirectToAction("Index", "PregledVracenih");


        }

        [Authorize]
        public ActionResult ProsljediKontroli(int id)
        {

            var kontakt = efContext.CC_KONTAKTI.Find(id);

            using (var transaction = efContext.Database.BeginTransaction())
            {
                try
                {
                    kontakt.TRENUTNO_GRUPA_ID = "6";

                    int results2 = efContext.SaveChanges();
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    _logger.Error("Greska na metodi ProsljediKontroli get." + " " + ex.Message);
                }
            }

            TempData["Success"] = "Uspješno ste prosljedili kontakt!";

            return RedirectToAction("Index", "PregledVracenih");


        }

        [Authorize]
        public ActionResult ProsljediProdaji(int id)
        {

            var kontakt = efContext.CC_KONTAKTI.Find(id);

            using (var transaction = efContext.Database.BeginTransaction())
            {
                try
                {
                    kontakt.TRENUTNO_GRUPA_ID = "3";

                    int results2 = efContext.SaveChanges();
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    _logger.Error("Greska na metodi ProsljediProdaji get." + " " + ex.Message);
                }
            }

            TempData["Success"] = "Uspješno ste prosljedili kontakt!";

            return RedirectToAction("Index", "PregledVracenih");


        }
        [Authorize]
        public ActionResult VratiMarketingu(int id)
        {

            var kontakt = efContext.CC_KONTAKTI.Find(id);

            using (var transaction = efContext.Database.BeginTransaction())
            {
                try
                {
                    kontakt.TRENUTNO_GRUPA_ID = "4";
                    kontakt.VRACEN_MARKETINGU = "Y";
                    kontakt.VRACENO_SA_KONTROLE = "Y";

                    int results2 = efContext.SaveChanges();
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    _logger.Error("Greska na metodi VratiMarketingu get." + " " + ex.Message);
                }
            }

            TempData["Success"] = "Uspješno ste vratili kontakt!";

            return RedirectToAction("Index", "PregledVracenih");


        }
        [Authorize]
        public ActionResult VratiAgentu(int id)
        {

            var kontakt = efContext.CC_KONTAKTI.Find(id);

            using (var transaction = efContext.Database.BeginTransaction())
            {
                try
                {
                    kontakt.TRENUTNO_GRUPA_ID = "2";
                    kontakt.VRACEN_MARKETINGU = "Y";
                    kontakt.VRACENO_SA_KONTROLE = null;

                    int results2 = efContext.SaveChanges();
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    _logger.Error("Greska na metodi VratiAgentu get." + " " + ex.Message);
                }
            }

            TempData["Success"] = "Uspješno ste vratili kontakt!";

            return RedirectToAction("Index", "PregledVracenih");


        }
    }
}