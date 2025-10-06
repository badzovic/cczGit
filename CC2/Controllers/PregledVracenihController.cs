using CC2.Models;
using DataAccess;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using NLog;
using CC2.Helpers;

namespace CC2.Controllers
{
    public class PregledVracenihController : BaseController
    {
        CCEntities efContext = new CCEntities();
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        // GET: PregledVracenih
   
        [Authorize]
        public ActionResult Index()
        {
            var successMessage = TempData["Success"] as string;

            // Check if it's not null
            if (!string.IsNullOrEmpty(successMessage))
            {
                // You can now use the successMessage in your view or wherever needed
                ViewBag.SuccessMessage = successMessage;
            }

            Pregled pregled = new Pregled();

            var userIds = efContext.AspNetUserRoles
              .Where(u => u.RoleId == "3")
              .Select(u => u.UserId)
              .ToList();

            var usersWithIds = efContext.AspNetUsers
            .Where(u => userIds.Contains(u.Id) && u.Active == "Y" && u.Deleted != "Y")
            .Select(u => new UserInfo
            {
                Id = u.Id,
                Email = u.Email
            })
            .ToList();




            pregled.Users = usersWithIds;


            var user = HttpContext.User;
            var username = user.Identity.Name;
            var userId = User.Identity.GetUserId();

            if (user.IsInRole("marketing"))
            {
                pregled.kontaktiMarketing = GetKontaktiSaTerminima(
                    efContext.CC_KONTAKTI
                        .Where(k => k.TRENUTNO_KOD_ID == userId
                                 && k.VRACEN_MARKETINGU == "Y"
                                 && k.NIJE_DOBIJEN != "Y"
                                 && k.PRODAT != "Y")
                        .OrderByDescending(k => k.ID)
                );
            }
            else if (user.IsInRole("adminmarketing"))
            {
                pregled.kontaktiAdminMarketing = GetKontaktiSaTerminima(
                    efContext.CC_KONTAKTI
                        .Where(k => k.TRENUTNO_KOD_ID == userId
                                 && k.VRACEN_MARKETINGU == "Y"
                                 && k.NIJE_DOBIJEN != "Y"
                                 && k.PRODAT != "Y")
                        .OrderByDescending(k => k.DATETIME_UPDATED)
                );
            }
            else if (user.IsInRole("adminsales"))
            {
                pregled.kontaktiSalesAdmin = GetKontaktiSaTerminima(
                    efContext.CC_KONTAKTI
  .Where(k => k.TRENUTNO_KOD_ID == userId
                                 && k.VRACEN_MARKETINGU == "Y"
                                 && k.NIJE_DOBIJEN != "Y"
                                 && k.PRODAT != "Y")
                        .OrderByDescending(k => k.ID)
                );
            }
            else if (user.IsInRole("sales"))
            {
                pregled.kontaktiSales = GetKontaktiSaTerminima(
                    efContext.CC_KONTAKTI
                         .Where(k => k.TRENUTNO_KOD_ID == userId
                                 && k.VRACEN_MARKETINGU == "Y"
                                 && k.NIJE_DOBIJEN != "Y"
                                 && k.PRODAT != "Y")
                        .OrderByDescending(k => k.DATETIME_CREATED)
                );
            }


            var roleIds = new[] { "2", "3", "5" };

            var agents = (from ur in efContext.AspNetUserRoles
                          join u in efContext.AspNetUsers on ur.UserId equals u.Id
                          where roleIds.Contains(ur.RoleId)
                                && u.Active == "Y"
                                && u.Deleted != "Y"
                          select new UserInfo
                          {
                              Id = u.Id,
                              Email = u.Email
                          }).ToList();

            pregled.Users = agents;


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
        private List<KontaktSaTerminima> GetKontaktiSaTerminima(IQueryable<CC_KONTAKTI> query)
        {
            return query
                .GroupJoin(
                    efContext.TERMINI,
                    kontakt => kontakt.ID,
                    termin => termin.KONTAKT_ID,
                    (kontakt, termini) => new { kontakt, termini }
                )
                .Select(x => new KontaktSaTerminima
                {
                    Kontakt = x.kontakt,
                    SljedeciTermin = x.termini
                        .Where(t => t.DATUM >= DateTime.Now)
                        .OrderBy(t => t.DATUM)
                        .Select(t => (DateTime?)t.DATUM)
                        .FirstOrDefault(),
                    SljedeciTerminId = x.termini
                    .Where(t => t.DATUM >= DateTime.Now)
                    .OrderBy(t => t.DATUM)
                    .Select(t => (int?)t.ID)
                    .FirstOrDefault()

                })
                .OrderBy(x => x.SljedeciTermin ?? DateTime.MaxValue)
                .ThenByDescending(x => x.Kontakt.ID)
                .ToList();
        }
    }
}