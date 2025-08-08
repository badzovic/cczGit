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


namespace CC2.Controllers
{
    public class PregledController : Controller
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
                pregled.kontaktiMarketing = efContext.CC_KONTAKTI
               .Where(k => k.TRENUTNO_KOD_ID == userId && k.TRENUTNO_GRUPA_ID == "2" && k.VRACEN_MARKETINGU == null && k.PRODAT != "Y").OrderByDescending(k => k.ID)
               .ToList();
            }
            else if (user.IsInRole("adminmarketing"))
            {
                pregled.kontaktiAdminMarketing = efContext.CC_KONTAKTI
               .Where(k => k.TRENUTNO_GRUPA_ID == "4" && k.VRACENO_SA_KONTROLE == null && k.PRODAT != "Y").OrderByDescending(k => k.DATETIME_UPDATED)
               .ToList();
            }
            else if (user.IsInRole("kontrola"))
            {
                pregled.kontrola = efContext.CC_KONTAKTI
               .Where(k => k.TRENUTNO_GRUPA_ID == "6" && k.PRODAT != "Y").OrderByDescending(k => k.ID)
               .ToList();
            }
            else if (user.IsInRole("adminsales"))
            {
                pregled.kontaktiSalesAdmin = efContext.CC_KONTAKTI
               .Where(k => k.TRENUTNO_GRUPA_ID == "5" && k.PRODAT != "Y" && k.SALES_NIJE_ZAINTERESOVAN != "Y" && k.U_PREGOVORIMA != "Y" && k.PRODAT != "Y" && k.NIJE_DOBIJEN != "Y").OrderByDescending(k => k.ID)
               .ToList();
            }
            else if (user.IsInRole("sales"))
            {
                pregled.kontaktiSales = efContext.CC_KONTAKTI
               .Where(k => k.TRENUTNO_GRUPA_ID == "3" && k.TRENUTNO_KOD_ID == userId && k.NIJE_DOBIJEN != "Y" && k.U_PREGOVORIMA != "Y" && k.PRODAT != "Y").OrderByDescending(k => k.DATETIME_CREATED)
               .ToList();
            }


            return View(pregled);

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
        public ActionResult SalesAdmin(string filter, string SelectedAgentId, bool clearFilters = false)
        {
            if (clearFilters)
            {
                // Očisti filtere iz sesije
                Session["SelectedFilter"] = null;
                Session["SelectedAgentId"] = null;
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
                if ((!string.IsNullOrEmpty(filter)) && (!string.IsNullOrEmpty(SelectedAgentId)))
                {
                    switch (filter)
                    {
                        case "U_PREGOVORIMA":
                            pregled.kontaktiSalesAdminSvi = efContext.CC_KONTAKTI.Where(k => k.U_PREGOVORIMA == "Y" && k.TRENUTNO_KOD_ID == SelectedAgentId).ToList();
                            break;
                        case "NIJE_DOBIJEN":
                            pregled.kontaktiSalesAdminSvi = efContext.CC_KONTAKTI.Where(k => k.NIJE_DOBIJEN == "Y" && k.TRENUTNO_KOD_ID == SelectedAgentId).ToList();
                            break;
                        case "NIJE_ZAINTERESOVAN":
                            pregled.kontaktiSalesAdminSvi = efContext.CC_KONTAKTI.Where(k => k.SALES_NIJE_ZAINTERESOVAN == "Y" && k.TRENUTNO_KOD_ID == SelectedAgentId).ToList();
                            break;
                        case "PRODAT":
                            pregled.kontaktiSalesAdminSvi = efContext.CC_KONTAKTI.Where(k => k.PRODAT == "Y" && k.TRENUTNO_KOD_ID == SelectedAgentId).ToList();
                            break;
                    }
                }
                else if (!string.IsNullOrEmpty(filter))
                {
                    switch (filter)
                    {
                        case "U_PREGOVORIMA":
                            pregled.kontaktiSalesAdminSvi = efContext.CC_KONTAKTI.Where(k => k.U_PREGOVORIMA == "Y").ToList();
                            break;
                        case "NIJE_DOBIJEN":
                            pregled.kontaktiSalesAdminSvi = efContext.CC_KONTAKTI.Where(k => k.NIJE_DOBIJEN == "Y").ToList();
                            break;
                        case "NIJE_ZAINTERESOVAN":
                            pregled.kontaktiSalesAdminSvi = efContext.CC_KONTAKTI.Where(k => k.SALES_NIJE_ZAINTERESOVAN == "Y").ToList();
                            break;
                        case "PRODAT":
                            pregled.kontaktiSalesAdminSvi = efContext.CC_KONTAKTI.Where(k => k.PRODAT == "Y").ToList()  ;
                            break;
                    }
                }
                else if (!string.IsNullOrEmpty(SelectedAgentId))
                {
                   pregled.kontaktiSalesAdminSvi = efContext.CC_KONTAKTI.Where(k => k.TRENUTNO_KOD_ID == SelectedAgentId).ToList();
                }
                else
                {
                    pregled.kontaktiSalesAdminSvi = efContext.CC_KONTAKTI
                                 .Where(k => k.TRENUTNO_GRUPA_ID == "5" || k.TRENUTNO_GRUPA_ID == "3").OrderByDescending(k => k.ID)
                                 .ToList();
                }            
            }

            ViewBag.SelectedFilter = filter;
            ViewBag.SelectedAgentId = SelectedAgentId;

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
}

