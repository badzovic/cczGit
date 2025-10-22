using CC2.Models;
using DataAccess;
using Microsoft.Ajax.Utilities;
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
    public class PregledController : BaseController
    {

        CCEntities efContext = new CCEntities();
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        // GET: Pregled
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
                                 && k.TRENUTNO_GRUPA_ID == "2"
                                 && k.VRACEN_MARKETINGU == null
                                 && k.PRODAT != "Y" && k.KIVVL != "DA")
                        .OrderByDescending(k => k.ID)
                );
            }
            else if (user.IsInRole("adminmarketing"))
            {
                pregled.kontaktiAdminMarketing = GetKontaktiSaTerminima(
                    efContext.CC_KONTAKTI
                        .Where(k => k.TRENUTNO_GRUPA_ID == "4"
                                 && k.VRACENO_SA_KONTROLE == null
                                 && k.PRODAT != "Y" && k.KIVVL != "DA")
                        .OrderByDescending(k => k.DATETIME_UPDATED)
                );
            }
            else if (user.IsInRole("kontrola"))
            {
                pregled.kontrola = GetKontaktiSaTerminima(
                    efContext.CC_KONTAKTI
                        .Where(k => k.TRENUTNO_GRUPA_ID == "6" && k.PRODAT != "Y" && k.KIVVL != "DA")
                        .OrderByDescending(k => k.ID)
                );
            }
            else if (user.IsInRole("adminsales"))
            {
                pregled.kontaktiSalesAdmin = GetKontaktiSaTerminima(
                    efContext.CC_KONTAKTI
                        .Where(k => k.TRENUTNO_GRUPA_ID == "5"
                                 && k.PRODAT != "Y"
                                 && k.SALES_NIJE_ZAINTERESOVAN != "Y"
                                 && k.U_PREGOVORIMA != "Y"
                                 && k.NIJE_DOBIJEN != "Y"
                                 && k.KIVVL != "DA")
                        .OrderByDescending(k => k.ID)
                );
            }
            else if (user.IsInRole("sales"))
            {
                pregled.kontaktiSales = GetKontaktiSaTerminima(
                    efContext.CC_KONTAKTI
                        .Where(k => k.TRENUTNO_GRUPA_ID == "3"
                                 && k.TRENUTNO_KOD_ID == userId
                                 && k.NIJE_DOBIJEN != "Y"
                                 && k.U_PREGOVORIMA != "Y"
                                 && k.PRODAT != "Y" && k.KIVVL != "DA")
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

        [Authorize]
        public ActionResult Kontakt(int id, int? page)
        {

            PregledKontakta pregled = new PregledKontakta();
            var user = HttpContext.User;
            var username = user.Identity.Name;
            var userId = User.Identity.GetUserId();

            pregled.pregledKontakta = efContext.CC_KONTAKTI
           .Where(k => k.ID == id)
           .ToList();

            return View(pregled);
        }

        [HttpPost]
        public ActionResult PrebaciTiket(int kontaktId, string agentId, int terminId, DateTime noviStart, DateTime noviEnd)
        {
            using (var efContext = new CCEntities())
            {
                var kontakt = efContext.CC_KONTAKTI.Find(kontaktId);
                if (kontakt == null)
                    return Json(new { success = false, message = "Kontakt ne postoji" });

                var agentRoleId = efContext.AspNetUserRoles
                .Where(r => r.UserId == agentId)
                .Select(r => r.RoleId)
                .FirstOrDefault();

                // prebaci kontakt
                kontakt.TRENUTNO_KOD_ID = agentId;
                kontakt.VRACEN_MARKETINGU = "Y";
                kontakt.NIJE_DOBIJEN = null;
                kontakt.PRODAT = null;
                kontakt.VVL_DATUM = null;
                // update grupe na osnovu role
                if (agentRoleId == "2") // marketing
                {
                    kontakt.TRENUTNO_GRUPA_ID = "2";
                }
                else if (agentRoleId == "5") // adminsales
                {
                    kontakt.TRENUTNO_GRUPA_ID = "5";
                }
                // prebaci termin
                var termin = efContext.TERMINI.FirstOrDefault(t => t.ID == terminId && t.KONTAKT_ID == kontaktId);
                if (termin != null)
                {
                    termin.USER_ID = agentId;
                    termin.DATUM = noviStart;
                    termin.DATUM_KRAJA = noviEnd;
                }

                efContext.SaveChanges();
                return Json(new { success = true });
            }
        }

        [HttpPost]
        public ActionResult PrebaciTiketNijeDobijen(int kontaktId, string agentId, int terminId, DateTime noviStart, DateTime noviEnd)
        {
            using (var efContext = new CCEntities())
            {
                var kontakt = efContext.CC_KONTAKTI.Find(kontaktId);
                if (kontakt == null)
                    return Json(new { success = false, message = "Kontakt ne postoji" });

                var agentRoleId = efContext.AspNetUserRoles
                .Where(r => r.UserId == agentId)
                .Select(r => r.RoleId)
                .FirstOrDefault();

                // prebaci kontakt
                kontakt.TRENUTNO_KOD_ID = agentId;
                kontakt.VRACEN_MARKETINGU = null;
                kontakt.NIJE_DOBIJEN = "Y";
                // update grupe na osnovu role
                if (agentRoleId == "2") // marketing
                {
                    kontakt.TRENUTNO_GRUPA_ID = "2";
                }
                else if (agentRoleId == "5") // adminsales
                {
                    kontakt.TRENUTNO_GRUPA_ID = "5";
                }
                // prebaci termin
                var termin = efContext.TERMINI.FirstOrDefault(t => t.ID == terminId && t.KONTAKT_ID == kontaktId);
                if (termin != null)
                {
                    termin.USER_ID = agentId;
                    termin.DATUM = noviStart;
                    termin.DATUM_KRAJA = noviEnd;
                }

                efContext.SaveChanges();
                return Json(new { success = true });
            }
        }


        [Authorize]
        public ActionResult Blacklist(int id)
        {

            PregledKontakta pregled = new PregledKontakta();
            var user = HttpContext.User;
            var username = user.Identity.Name;
            var userId = User.Identity.GetUserId();


            var kontakt = efContext.CC_KONTAKTI.Find(id);

            using (var transaction = efContext.Database.BeginTransaction())
            {
                try
                {             
                    kontakt.BLACKLIST = "Y";
                    int results = efContext.SaveChanges();
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    _logger.Error("Greska na metodi Blacklist get." + " " + ex.Message);
                }
            }
            TempData["Success"] = "Uspješno ste oznacili blacklist kontakt!";
            return RedirectToAction("Index", "Pregled");
        }
        [Authorize]
        public ActionResult SalesAdmin(string filter, string SelectedAgentId, string vrstaProdajeFilter, bool clearFilters = false)
        {
            if (clearFilters)
            {
                // Očisti filtere iz sesije
                Session["SelectedFilter"] = null;
                Session["SelectedAgentId"] = null;
                Session["SelectedVrstaProdaje"] = null; 
                filter = null;
                SelectedAgentId = null;
            }
            else
            {
                if (!string.IsNullOrEmpty(filter))
                {
                    Session["SelectedFilter"] = filter;
                }
                else
                {
                    filter = Session["SelectedFilter"] as string;
                }

                if (!string.IsNullOrEmpty(SelectedAgentId))
                {
                    Session["SelectedAgentId"] = SelectedAgentId;
                }
                if (!string.IsNullOrEmpty(vrstaProdajeFilter))
                    Session["SelectedVrstaProdaje"] = vrstaProdajeFilter;
                else
                {
                    SelectedAgentId = Session["SelectedAgentId"] as string;
                }
            }

            Pregled pregled = new Pregled();

            var user = HttpContext.User;
            var username = user.Identity.Name;
            var userId = User.Identity.GetUserId();

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

            if (user.IsInRole("adminsales"))
            {
                IQueryable<CC_KONTAKTI> query = efContext.CC_KONTAKTI;

                if ((!string.IsNullOrEmpty(filter)) && (!string.IsNullOrEmpty(SelectedAgentId)))
                {
                    switch (filter)
                    {
                        case "U_PREGOVORIMA":
                            query = query.Where(k => k.U_PREGOVORIMA == "Y" && k.TRENUTNO_KOD_ID == SelectedAgentId);
                            break;
                        case "NIJE_DOBIJEN":
                            query = query.Where(k => k.NIJE_DOBIJEN == "Y" && k.TRENUTNO_KOD_ID == SelectedAgentId);
                            break;
                        case "NIJE_ZAINTERESOVAN":
                            query = query.Where(k => k.SALES_NIJE_ZAINTERESOVAN == "Y" && k.TRENUTNO_KOD_ID == SelectedAgentId);
                            break;
                        case "PRODAT":
                            query = query.Where(k => k.PRODAT == "Y" && k.TRENUTNO_KOD_ID == SelectedAgentId);
                            break;
                    }
                }
                else if (!string.IsNullOrEmpty(filter))
                {
                    switch (filter)
                    {
                        case "U_PREGOVORIMA":
                            query = query.Where(k => k.U_PREGOVORIMA == "Y");
                            break;
                        case "NIJE_DOBIJEN":
                            query = query.Where(k => k.NIJE_DOBIJEN == "Y");
                            break;
                        case "NIJE_ZAINTERESOVAN":
                            query = query.Where(k => k.SALES_NIJE_ZAINTERESOVAN == "Y");
                            break;
                        case "PRODAT":
                            query = query.Where(k => k.PRODAT == "Y");
                            break;
                    }
                }
                else if (!string.IsNullOrEmpty(SelectedAgentId))
                {
                    query = query.Where(k => k.TRENUTNO_KOD_ID == SelectedAgentId);
                }
                else
                {
                    query = query.Where(k => k.TRENUTNO_GRUPA_ID == "5" || k.TRENUTNO_GRUPA_ID == "3");
                }
                if (!string.IsNullOrEmpty(vrstaProdajeFilter))
                {
                    query = query.Where(k => k.VRSTA_PRODAJE == vrstaProdajeFilter);
                }

                pregled.kontaktiSalesAdminSvi = query
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
                         .Where(t => t.DATUM >= DateTime.Now)   // samo budući termini
                         .OrderBy(t => t.DATUM)
                         .Select(t => (DateTime?)t.DATUM)
                         .FirstOrDefault()
                 })
                 .OrderBy(x => x.SljedeciTermin ?? DateTime.MaxValue)
                 .ThenByDescending(x => x.Kontakt.ID)
                 .ToList();
            }

            ViewBag.SelectedFilter = filter;
            ViewBag.SelectedAgentId = SelectedAgentId;
            ViewBag.SelectedVrstaProdaje = vrstaProdajeFilter;

            return View(pregled);
        }

        [Authorize]
        public ActionResult kontaktSales(int id)
        {

            PregledKontakta pregled = new PregledKontakta();
            var user = HttpContext.User;
            var username = user.Identity.Name;
            var userId = User.Identity.GetUserId();

            pregled.pregledKontakta = efContext.CC_KONTAKTI
           .Where(k => k.ID == id)
           .ToList();

            return View(pregled);
        }
        public ActionResult Prosljedi(string selectedIds)
        {
            

            string[] idStrings = selectedIds.Split(',');

            int[] idArray = new int[idStrings.Length];

            for (int i = 0; i < idStrings.Length; i++)
            {
                if (int.TryParse(idStrings[i], out int id))
                {
                    idArray[i] = id;
                }
            }

            using (var transaction = efContext.Database.BeginTransaction())
            {
                try
                {
                    // Loop through the selected IDs and process each item
                    foreach (var id in idArray)
                    {
                        var kontakt = efContext.CC_KONTAKTI.Find(id);
                        kontakt.TRENUTNO_GRUPA_ID = "5";
                        kontakt.DATETIME_UPDATED = DateTime.Now;
                        kontakt.VRACEN_MARKETINGU = null;
                        kontakt.NIJE_DOBIJEN = null;
                    }

                    int results = efContext.SaveChanges();
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    _logger.Error("Greska na metodi Prosljedi get." + " " + ex.Message);
                }
            }

            TempData["Success"] = "Uspješno ste prosljedili kontakte!";

            return RedirectToAction("Index", "Pregled");

        }

        [Authorize]
        public ActionResult nijeZainteresovan(int id)
        {

            var kontakt = efContext.CC_KONTAKTI.Find(id);
            var user = HttpContext.User;
            var username = user.Identity.Name;
            var userId = User.Identity.GetUserId();

            using (var transaction = efContext.Database.BeginTransaction())
            {
                try
                {
                    kontakt.TRENUTNO_GRUPA_ID = "5";
                    kontakt.SALES_NIJE_ZAINTERESOVAN = "Y";
                    kontakt.SALES_NIJE_ZAINTERESOVAN_DATE = DateTime.Now;
                    int results2 = efContext.SaveChanges();
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    _logger.Error("Greska na metodi nijeZainteresovan get." + " " + ex.Message);
                }
            }

            

            if (user.IsInRole("adminsales"))
            {
                TempData["Success"] = "Uspješno ste oznacili kontakt kao nezainteresovan!";
                return RedirectToAction("SalesAdmin", "Pregled");
            }
            TempData["Success"] = "Uspješno ste oznacili kontakt kao nezainteresovan!";

            return RedirectToAction("Index", "Pregled");


        }

        [Authorize]
        public ActionResult uPregovorima(int id)
        {

            var kontakt = efContext.CC_KONTAKTI.Find(id);
            var user = HttpContext.User;
            var username = user.Identity.Name;
            var userId = User.Identity.GetUserId();

            using (var transaction = efContext.Database.BeginTransaction())
            {
                try
                {
                    kontakt.NIJE_DOBIJEN = null;
                    kontakt.U_PREGOVORIMA = "Y";
                    kontakt.DATETIME_UPDATED = DateTime.Now;
                    int results2 = efContext.SaveChanges();
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    _logger.Error("Greska na metodi uPregovorima get." + " " + ex.Message);

                }
            }



            if (user.IsInRole("adminsales"))
            {
                TempData["Success"] = "Uspješno ste oznacili kontakt kao u pregovorima!";

                return RedirectToAction("SalesAdmin", "Pregled");
            }

            TempData["Success"] = "Uspješno ste oznacili kontakt kao u pregovorima!";

            return RedirectToAction("Index", "Pregled");


        }


        [Authorize]
        public ActionResult kivvl(int id)
        {

            var kontakt = efContext.CC_KONTAKTI.Find(id);
            var user = HttpContext.User;
            var username = user.Identity.Name;
            var userId = User.Identity.GetUserId();

            using (var transaction = efContext.Database.BeginTransaction())
            {
                try
                {
                    kontakt.NIJE_DOBIJEN = null;
                    kontakt.KIVVL = "DA";
                    kontakt.TRENUTNO_GRUPA_ID = "5";
                    kontakt.TRENUTNO_KOD_ID = "50fbd40f-2379-49cd-9776-dc2fad1fa562";
                    kontakt.DATETIME_UPDATED = DateTime.Now;
                    int results2 = efContext.SaveChanges();
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    _logger.Error("Greska na metodi KIVVL get." + " " + ex.Message);

                }
            }



            if (user.IsInRole("adminsales"))
            {
                TempData["Success"] = "Uspješno ste oznacili kontakt kao u KI VVL!";

                return RedirectToAction("SalesAdmin", "Pregled");
            }

            TempData["Success"] = "Uspješno ste oznacili kontakt kao u KI VVL!";

            return RedirectToAction("Index", "Pregled");


        }

        [Authorize]
        [HttpPost]
        public JsonResult ObrisiKontakt(int id)
        {
            var userId = User.Identity.GetUserId();
            var username = User.Identity.Name;

            using (var transaction = efContext.Database.BeginTransaction())
            {
                try
                {
                    var kontakt = efContext.CC_KONTAKTI.Find(id);
                    if (kontakt == null)
                    {
                        return Json(new { success = false, message = "Kontakt nije pronađen." });
                    }

                    var log = new CC_KONTAKTI_OBRISANI
                    {
                        ORIGINAL_ID = kontakt.ID,
                        FIRMA = kontakt.FIRMA,
                        IME = kontakt.IME,
                        PREZIME = kontakt.PREZIME,
                        ADRESA = kontakt.ADRESA,
                        PLZ = kontakt.PLZ,
                        DRZAVA = kontakt.DRZAVA,
                        GRAD = kontakt.GRAD,
                        PREFIX = kontakt.PREFIX,
                        BROJ = kontakt.BROJ,
                        EMAIL = kontakt.EMAIL,
                        VRSTA_PRODAJE = kontakt.VRSTA_PRODAJE,
                        BROJ_KARTICA = kontakt.BROJ_KARTICA,
                        VVL_DATUM = kontakt.VVL_DATUM,
                        KOMENTAR = kontakt.KOMENTAR ?? kontakt.KOMENTAR2,
                        DATETIME_OBRISAN = DateTime.Now,
                        OBRISAO_ID = userId,
                        OBRISAO_USERNAME = username
                    };

                    efContext.CC_KONTAKTI_OBRISANI.Add(log);

                    efContext.CC_KONTAKTI.Remove(kontakt);

                    efContext.SaveChanges();
                    transaction.Commit();

                    return Json(new { success = true });
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    return Json(new { success = false, message = ex.Message });
                }
            }
        }


        [Authorize]
        public ActionResult nijeZainteresovanSales(int id)
        {

            var kontakt = efContext.CC_KONTAKTI.Find(id);
            var user = HttpContext.User;
            var username = user.Identity.Name;
            var userId = User.Identity.GetUserId();

            using (var transaction = efContext.Database.BeginTransaction())
            {
                try
                {
                    kontakt.NIJE_DOBIJEN = null;
                    kontakt.SALES_NIJE_ZAINTERESOVAN = "Y";
                    kontakt.U_PREGOVORIMA = null;
                    kontakt.NIJE_DOBIJEN = null;
                    kontakt.TRENUTNO_KOD_ID = "50fbd40f-2379-49cd-9776-dc2fad1fa562";
                    kontakt.TRENUTNO_GRUPA_ID = "5";
                    kontakt.SALES_NIJE_ZAINTERESOVAN_DATE = DateTime.Now;
                    kontakt.DATETIME_UPDATED = DateTime.Now;
                    int results2 = efContext.SaveChanges();
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    _logger.Error("Greska na metodi nijeZainteresovanSales get." + " " + ex.Message);
                }
            }


            if (user.IsInRole("adminsales"))
            {
                TempData["Success"] = "Uspješno ste oznacili kontakt kao Nije Zainteresovan!";

                return RedirectToAction("SalesAdmin", "Pregled");
            }

            TempData["Success"] = "Uspješno ste oznacili kontakt kao Nije Zainteresovan!";

            return RedirectToAction("Index", "Pregled");


        }

        [Authorize]
        [HttpPost]
        public JsonResult NijeDobijen(int id)
        {
            var kontakt = efContext.CC_KONTAKTI.Find(id);
            if (kontakt == null)
                return Json(new { success = false, message = "Kontakt nije pronađen." });

            var user = HttpContext.User;

            using (var transaction = efContext.Database.BeginTransaction())
            {
                try
                {
                    kontakt.TRENUTNO_GRUPA_ID = "2";
                    kontakt.TRENUTNO_KOD_ID = kontakt.KREIRAO_ID;
                    kontakt.U_PREGOVORIMA = null;
                    kontakt.NIJE_DOBIJEN = "Y";
                    kontakt.DATETIME_UPDATED = DateTime.Now;
                    kontakt.VRACEN_MARKETINGU = "Y";
                    efContext.SaveChanges();
                    transaction.Commit();

                    // Vrati success i gdje redirektovati
                    var redirectUrl = user.IsInRole("adminsales") ? "/Pregled/SalesAdmin" : "/Pregled";

                    return Json(new { success = true, redirectUrl });
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, message = ex.Message });
                }
            }
        }


        [Authorize]
        [HttpPost]
        public JsonResult prodat(int id, DateTime vvlDatum, string vrstaProdaje)
        {
            var kontakt = efContext.CC_KONTAKTI.Find(id);
            var userId = User.Identity.GetUserId();
            var username = User.Identity.Name;

            using (var transaction = efContext.Database.BeginTransaction())
            {
                try
                {
                    kontakt.PRODAT = "Y";
                    kontakt.TRENUTNO_GRUPA_ID = "5";
                    kontakt.TRENUTNO_KOD_ID = "50fbd40f-2379-49cd-9776-dc2fad1fa562";
                    kontakt.FINALIZIRAO_ID = userId;
                    kontakt.DATETIME_UPDATED = DateTime.Now;
                    kontakt.VRSTA_PRODAJE = vrstaProdaje;
                    kontakt.VVL_DATUM = vvlDatum;


                    var historyEntry = new KONTAKT_HISTORY
                    {
                        KONTAKT_ID = id,
                        STATUS = "prodat",
                        CREATED_AT = DateTime.Now,
                        CHANGED_AT = DateTime.Now
                    };

                    efContext.KONTAKT_HISTORY.Add(historyEntry);
                    var ok = efContext.SaveChanges();

                    transaction.Commit();

                    _logger.Info($"Korisnik {username} je označio kontakt {id} kao PRODAT (VVL datum: {vvlDatum:dd.MM.yyyy}).");

                    return Json(new { success = true });
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    _logger.Error("Greška u metodi prodat: " + ex.Message);
                    return Json(new { success = false, message = ex.Message });
                }
            }
        }


        [Authorize]
        public ActionResult prodat(int id)
        {

            var kontakt = efContext.CC_KONTAKTI.Find(id);
            var userId = User.Identity.GetUserId();
            var username = User.Identity.Name;

            using (var transaction = efContext.Database.BeginTransaction())
            {
                try
                {

                    kontakt.PRODAT = "Y";
                    kontakt.TRENUTNO_GRUPA_ID = "5";
                    kontakt.TRENUTNO_KOD_ID = "50fbd40f-2379-49cd-9776-dc2fad1fa562";
                    kontakt.FINALIZIRAO_ID = userId;
                    kontakt.DATETIME_UPDATED = DateTime.Now;
                    int results2 = efContext.SaveChanges();
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    _logger.Error("Greska na metodi prodat get." + " " + ex.Message);
                }
            }

            _logger.Info("Korisnik" + " " + username + " " + "je uspješno oznacio prodaju za id " + " " + id);

            TempData["Success"] = "Čestitamo! Uspješno ste oznacili kontakt kao PRODAT!";

            return RedirectToAction("Index", "Pregled");
        }

        [Authorize]
        public ActionResult ProsljediKontroli(string selectedIds)
        {

            string[] idStrings = selectedIds.Split(',');

            int[] idArray = new int[idStrings.Length];

            for (int i = 0; i < idStrings.Length; i++)
            {
                if (int.TryParse(idStrings[i], out int id))
                {
                    idArray[i] = id;
                }
            }

            using (var transaction = efContext.Database.BeginTransaction())
            {
                try
                {
                    // Loop through the selected IDs and process each item
                    foreach (var id in idArray)
                    {
                        var kontakt = efContext.CC_KONTAKTI.Find(id);
                        kontakt.TRENUTNO_GRUPA_ID = "6";
                    }

                    int results = efContext.SaveChanges();
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    _logger.Error("Greska na metodi ProsljediKontroli get." + " " + ex.Message);
                }
            }

            TempData["Success"] = "Uspješno ste prosljedili kontakte!";

            return RedirectToAction("Index", "Pregled");
        }


        [Authorize]
        public ActionResult ProsljediProdaji(string selectedIds)
        {

            string[] idStrings = selectedIds.Split(',');

            int[] idArray = new int[idStrings.Length];

            for (int i = 0; i < idStrings.Length; i++)
            {
                if (int.TryParse(idStrings[i], out int id))
                {
                    idArray[i] = id;
                }
            }

            using (var transaction = efContext.Database.BeginTransaction())
            {
                try
                {
                    // Loop through the selected IDs and process each item
                    foreach (var id in idArray)
                    {
                        var kontakt = efContext.CC_KONTAKTI.Find(id);
                        kontakt.TRENUTNO_GRUPA_ID = "5";
                    }

                    int results = efContext.SaveChanges();
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    _logger.Error("Greska na metodi ProsljediProdaji get." + " " + ex.Message);
                }
            }

            TempData["Success"] = "Uspješno ste prosljedili kontakte!";

            return RedirectToAction("Index", "Pregled");

        }
        [Authorize]
        public ActionResult Mojikreirani()
        {
            Pregled pregled = new Pregled();

          


            var user = HttpContext.User;
            var username = user.Identity.Name;
            var userId = User.Identity.GetUserId();

            if (user.IsInRole("marketing"))
            {
                pregled.kontaktiMarketingMoji = efContext.CC_KONTAKTI
               .Where(k => k.KREIRAO_ID == userId).OrderByDescending(k => k.ID)
               .ToList();
            }

            return View(pregled);
        }
        [Authorize]
        public ActionResult ProsljediAgentSalesu(string selectedIds, string selectedAgentId, string z = null)
        {
            var selectedIdsArray = selectedIds.Split(',').Select(int.Parse).ToList();
            var user = HttpContext.User;
            var username = user.Identity.Name;
            var userId = User.Identity.GetUserId();

            using (var transaction = efContext.Database.BeginTransaction())
            {
                try
                {
                    foreach (var selectedId in selectedIdsArray)
                    {
                        var kontakt = efContext.CC_KONTAKTI.Find(selectedId);
                        if (kontakt != null)
                        {
                            kontakt.TRENUTNO_GRUPA_ID = "3";
                            kontakt.TRENUTNO_KOD_ID = selectedAgentId;
                            kontakt.SALES_NIJE_ZAINTERESOVAN = null;
                            kontakt.NIJE_DOBIJEN = null;
                            kontakt.U_PREGOVORIMA = null;
                            kontakt.DATUM_DODJELE = DateTime.Now;


                            var termin = efContext.TERMINI
                             .FirstOrDefault(t => t.KONTAKT_ID == selectedId);

                            if (termin != null)
                            {
                                termin.USER_ID = selectedAgentId; 
                            }

                        }
                    }

                    efContext.SaveChanges();
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    _logger.Error("Greska na metodi ProsljediAgentSalesu get." + " " + ex.Message);
                }
            }

            if (user.IsInRole("adminsales"))
            {
                TempData["Success"] = "Uspješno ste prosljedili kontakte agentu!";
                if (z == "1")
                {
                    return RedirectToAction("Index", "Nijezainteresovan");
                }
                else if (z == "2")
                {
                    return RedirectToAction("SalesAdmin", "Pregled");
                }

                return RedirectToAction("Index", "Pregled");
            }

            TempData["Success"] = "Uspješno ste prosljedili kontakte agentu!";

           

            return RedirectToAction("Index", "Pregled");
        }
        [Authorize]
        public ActionResult VratiMarketingu(string selectedIds)
        {

            string[] idStrings = selectedIds.Split(',');

            int[] idArray = new int[idStrings.Length];

            for (int i = 0; i < idStrings.Length; i++)
            {
                if (int.TryParse(idStrings[i], out int id))
                {
                    idArray[i] = id;
                }
            }

            using (var transaction = efContext.Database.BeginTransaction())
            {
                try
                {
                    // Loop through the selected IDs and process each item
                    foreach (var id in idArray)
                    {
                        var kontakt = efContext.CC_KONTAKTI.Find(id);
                        kontakt.TRENUTNO_GRUPA_ID = "4";
                        kontakt.VRACEN_MARKETINGU = "Y";
                        kontakt.VRACENO_SA_KONTROLE = "Y";

                    }

                    int results = efContext.SaveChanges();
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    _logger.Error("Greska na metodi Vratimarketingu get." + " " + ex.Message);
                }
            }

            TempData["Success"] = "Uspješno ste vratili kontakt!";

            return RedirectToAction("Index", "Pregled");

        }
        [Authorize]
        public ActionResult izbrisi(string selectedIds)
        {

            string[] idStrings = selectedIds.Split(',');

            int[] idArray = new int[idStrings.Length];

            for (int i = 0; i < idStrings.Length; i++)
            {
                if (int.TryParse(idStrings[i], out int id))
                {
                    idArray[i] = id;
                }
            }

            using (var transaction = efContext.Database.BeginTransaction())
            {
                try
                {
                    // Loop through the selected IDs and process each item
                    foreach (var id in idArray)
                    {
                        var kontakt = efContext.CC_KONTAKTI.Find(id);
                        if (kontakt != null)
                        {
                            efContext.CC_KONTAKTI.Remove(kontakt);
                        }

                    }

                    int results = efContext.SaveChanges();
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    _logger.Error("Greska na metodi izbrisi get." + " " + ex.Message);
                }
            }

            TempData["Success"] = "Uspješno ste izbrisali kontakt!";

            return RedirectToAction("Index", "Pregled");

        }
        [Authorize]
        public ActionResult VratiAgentu(string selectedIds)
        {
            string[] idStrings = selectedIds.Split(',');

            int[] idArray = new int[idStrings.Length];


            for (int i = 0; i < idStrings.Length; i++)
            {
                if (int.TryParse(idStrings[i], out int id))
                {
                    idArray[i] = id;
                }
            }

            using (var transaction = efContext.Database.BeginTransaction())
            {
                try
                {
                    // Loop through the selected IDs and process each item
                    foreach (var id in idArray)
                    {
                        var kontakt = efContext.CC_KONTAKTI.Find(id);
                        kontakt.TRENUTNO_GRUPA_ID = "2";
                        kontakt.VRACEN_MARKETINGU = "Y";

                        int results2 = efContext.SaveChanges();
                        transaction.Commit();
                    }

                    int results = efContext.SaveChanges();
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    _logger.Error("Greska na metodi VratiAgentu get." + " " + ex.Message);
                }
            }

            TempData["Success"] = "Uspješno ste vratili kontakt!";

            return RedirectToAction("Index", "Pregled");

        }



    }

    public class KontaktSaTerminima
    {
        public CC_KONTAKTI Kontakt { get; set; }
        public DateTime? SljedeciTermin { get; set; }
        public int? SljedeciTerminId { get; set; }   
    }

    

}

