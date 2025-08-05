using CC2.Models;
using DataAccess;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using NLog;

namespace CC2.Controllers
{
    public class HomeController : Controller
    {
        CCEntities efContext = new CCEntities();
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();


        [Authorize]
        public ActionResult Index()
        {


            string roleId = null; // Declare roleId outside the if block

            var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(new ApplicationDbContext()));

            Dashboard dashboard = new Dashboard();
            var datum = DateTime.Now;

            var user = HttpContext.User;
            var username = user.Identity.Name;
            var userId = User.Identity.GetUserId();
            var userRoles = userManager.GetRoles(userId);

            _logger.Info("Korisnik" + " " + username + " " + "se ulogovao na aplikaciju");

            if (userRoles != null && userRoles.Any())
            {
                var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(new ApplicationDbContext()));

                var roleName = userRoles[0]; // Assuming the user has one role, change as needed
                var role = roleManager.FindByName(roleName);

                if (role != null)
                {
                    roleId = role.Id;
                }
            }

            dashboard.uPregovorima = efContext.CC_KONTAKTI
               .Where(x => x.U_PREGOVORIMA == "Y" && x.PRODAT != "Y" && x.SALES_NIJE_ZAINTERESOVAN != "Y")
               .ToList();

            dashboard.uPregovorimaUser = efContext.CC_KONTAKTI
              .Where(x => x.U_PREGOVORIMA == "Y" && x.PRODAT != "Y" && x.SALES_NIJE_ZAINTERESOVAN != "Y" && x.TRENUTNO_KOD_ID == userId )
              .ToList();

            dashboard.prodati = efContext.CC_KONTAKTI
              .Where(x => x.PRODAT == "Y" && x.U_PREGOVORIMA != "Y" && x.SALES_NIJE_ZAINTERESOVAN != "Y")
              .ToList();

            dashboard.prodatiUser = efContext.CC_KONTAKTI
              .Where(x => x.PRODAT == "Y" && x.U_PREGOVORIMA != "Y" && x.SALES_NIJE_ZAINTERESOVAN != "Y" && x.FINALIZIRAO_ID == userId)
              .ToList();

            dashboard.nisuZainteresovani = efContext.CC_KONTAKTI
             .Where(x => x.SALES_NIJE_ZAINTERESOVAN == "Y" && x.U_PREGOVORIMA != "Y" && x.PRODAT != "Y")
             .ToList();

            dashboard.nisuZainteresovaniUser = efContext.CC_KONTAKTI
              .Where(x => x.SALES_NIJE_ZAINTERESOVAN == "Y" && x.U_PREGOVORIMA != "Y" && x.PRODAT != "Y" && x.TRENUTNO_KOD_ID == userId)
              .ToList();


            var initialTermini = efContext.TERMINI
               .Where(x => x.USER_ID == userId && x.DATUM >= datum)
               .ToList();

            var filteredTermini = initialTermini
                .Where(ter => efContext.CC_KONTAKTI
                    .Any(kontakt => kontakt.ID == ter.KONTAKT_ID && kontakt.SALES_NIJE_ZAINTERESOVAN != "Y"))
                .ToList();

            var distinctTermini = filteredTermini
                .GroupBy(ter => ter.KONTAKT_ID)
                .Select(g => g.OrderByDescending(ter => ter.DATUM).FirstOrDefault())
                .OrderBy(ter => ter.DATUM)
                .ToList();

            //var termini = efContext.TERMINI.Where(x => x.USER_ID == userId).Where(x => x.DATUM >= datum).OrderBy(x => x.DATUM).ToList() ?? null;

            var klijenti = efContext.CC_KONTAKTI
              .Where(k =>  k.KREIRAO_ID == userId).OrderByDescending(k => k.ID)
              .ToList();

            var vraceni = efContext.CC_KONTAKTI
              .Where(k => k.VRACEN_MARKETINGU == "Y" || k.VRACENO_SA_KONTROLE == "Y").OrderByDescending(k => k.ID)
              .ToList();

            DateTime lastDayOfYear = new DateTime(DateTime.Now.Year, 12, 31);


            var last12Months = Enumerable.Range(0, 12).Select(i => lastDayOfYear.AddMonths(-i)).ToList();

            var contactCounts = last12Months
            .Select(month => new
            {
                Month = month.ToString("MMMM yyyy"),
                Count = efContext.CC_KONTAKTI
                    .Where(contact =>
                        DbFunctions.TruncateTime(contact.DATETIME_CREATED).Value.Month == month.Month && // Compare with month.Month
                        DbFunctions.TruncateTime(contact.DATETIME_CREATED).Value.Year == month.Year) // Compare with month.Year
                    .Count()
            })
            .ToList();

            var jsonData = JsonConvert.SerializeObject(contactCounts);
            dashboard.graphData = jsonData;
            dashboard.ukupnoVraceniKlijenti = vraceni.Count();
            dashboard.ukupnoKlijenti = klijenti.Count();
            dashboard.zakazanihTermina = distinctTermini.Count();
            dashboard.termini = distinctTermini ?? null;


            return View(dashboard);


         
        }
    }
}