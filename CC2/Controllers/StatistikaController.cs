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
    public class StatistikaController : BaseController
    {
        CCEntities efContext = new CCEntities();

        [Authorize]
        public ActionResult Index()
        {
            var stat = efContext.STATISTIKA.ToList();

            ViewBag.StatData = stat;

            return View();
        }

        public ActionResult Init()
        {


            var pregled = new Statistika(); //
                                            //var mails = efContext.AspNetUsers.Select(m => new SelectListItem { Value = m.Id, Text = m.Email }).ToList();

            var roleId = "2";

            // Get all UserId values from AspNetUserRoles where RoleId == 2
            var userIds = efContext.AspNetUserRoles
                .Where(ur => ur.RoleId == roleId)
                .Select(ur => ur.UserId)
                .ToList();

            // Get a list of Id and Email for users with the retrieved UserId values
            var usersWithRole = efContext.AspNetUsers
                .Where(user => userIds.Contains(user.Id))
                .Select(user => new { user.Id, user.Email })
                .ToList();

            // Create a list of SelectListItem from the selected user data
            var userListItems = usersWithRole
                .Select(user => new SelectListItem { Value = user.Id, Text = user.Email })
                .ToList();


            pregled.sviKorisnici = userListItems;

            return View(pregled);
        }


        [Authorize]
        [HttpGet]
        public ActionResult Pregled(DateTime? dateFrom, DateTime? dateTo)
        {
            using (var efContext = new CCEntities())
            {
                if (!dateFrom.HasValue)
                    dateFrom = new DateTime(2024, 1, 1);
                if (!dateTo.HasValue)
                    dateTo = DateTime.Now;

                var model = new Dictionary<string, List<AgentStatsViewModel>>
        {
            { "Marketing", new List<AgentStatsViewModel>() },
            { "Sales", new List<AgentStatsViewModel>() }
        };

                // --- Aktivni korisnici i njihove role ---
                var activeUsers = efContext.AspNetUsers
                    .Join(efContext.AspNetUserRoles, u => u.Id, ur => ur.UserId,
                        (u, ur) => new { User = u, ur.RoleId })
                    .Where(x => x.User.Active == "Y" && x.User.Deleted != "Y")
                    .ToList();

                // --- Korisnici po rolama (string poređenje) ---
                var marketingUserIds = activeUsers
                    .Where(x => x.RoleId == "2") // marketing
                    .Select(x => x.User.Id)
                    .ToList();

                var salesUserIds = activeUsers
                    .Where(x => x.RoleId == "3") // sales
                    .Select(x => x.User.Id)
                    .ToList();

                // --- MARKETING STATISTIKA ---
                if (User.IsInRole("marketing") || User.IsInRole("adminsales"))
                {
                    var marketingStats = efContext.CC_KONTAKTI
                        .Where(k => marketingUserIds.Contains(k.KREIRAO_ID)
                            && k.DATETIME_CREATED >= dateFrom.Value
                            && k.DATETIME_CREATED <= dateTo.Value)
                        .GroupBy(k => k.KREIRAO_ID)
                        .Select(g => new AgentStatsViewModel
                        {
                            AgentName = efContext.AspNetUsers
                                .Where(u => u.Id == g.Key)
                                .Select(u => u.UserName)
                                .FirstOrDefault(),

                            TotalCreated = g.Count(),

                            // --- PRODAT iz KONTAKT_HISTORY ---
                            TotalProdato = efContext.KONTAKT_HISTORY
                                .Count(h => h.STATUS == "prodat" && g.Select(k => (int?)k.ID).Contains(h.KONTAKT_ID)),

                            TotalPregovori = g.Count(k =>
                               k.U_PREGOVORIMA == "Y" &&
                               k.VRACEN_MARKETINGU != "Y" &&
                               !efContext.KONTAKT_HISTORY.Any(h => h.KONTAKT_ID == k.ID && h.STATUS == "prodat")),

                            // --- NIJE ZAINTERESOVAN (Y), ali nije prodan ---
                            TotalNijeZainteresovan = g.Count(k =>
                                k.SALES_NIJE_ZAINTERESOVAN == "Y" &&
                                !efContext.KONTAKT_HISTORY.Any(h => h.KONTAKT_ID == k.ID && h.STATUS == "prodat")),


                            // --- OTVOREN (ni u jednoj od gornjih kategorija) ---
                            TotalOtvoren = g.Count(k =>
                                (k.U_PREGOVORIMA != "Y" && k.VRACEN_MARKETINGU != "Y") &&
                                k.SALES_NIJE_ZAINTERESOVAN != "Y" &&
                                !efContext.KONTAKT_HISTORY.Any(h => h.KONTAKT_ID == k.ID && h.STATUS == "prodat"))
                        })
                        .Where(x => x.AgentName != null)
                        .OrderBy(x => x.AgentName)
                        .ToList();

                    model["Marketing"] = marketingStats;
                }



                //
                // --- SALES STATISTIKA ---
                //
                if (User.IsInRole("sales") || User.IsInRole("adminsales") || User.IsInRole("adminmarketing"))
                {
                    var salesStats = efContext.CC_KONTAKTI
                        .Where(k => salesUserIds.Contains(k.FINALIZIRAO_ID)
                            && k.DATETIME_CREATED >= dateFrom.Value
                            && k.DATETIME_CREATED <= dateTo.Value)
                        .GroupBy(k => k.FINALIZIRAO_ID)
                        .Select(g => new AgentStatsViewModel
                        {
                            AgentName = efContext.AspNetUsers
                                .Where(u => u.Id == g.Key)
                                .Select(u => u.UserName)
                                .FirstOrDefault(),

                            // --- PRODAT iz KONTAKT_HISTORY ---
                            TotalSold = efContext.KONTAKT_HISTORY
                                .Count(h => h.STATUS == "prodat" && g.Select(x => (int?)x.ID).Contains(h.KONTAKT_ID)),

                            // --- VRACENO MARKETINGU (iz CC_KONTAKTI), ali ne prodat ---
                            TotalReturned = g.Count(k =>
                                k.VRACEN_MARKETINGU == "Y" &&
                                !efContext.KONTAKT_HISTORY.Any(h => h.KONTAKT_ID == k.ID && h.STATUS == "prodat")),

                            // --- NIJE ZAINTERESOVAN (iz CC_KONTAKTI), ali ne prodat ---
                            TotalNotInterested = g.Count(k =>
                                k.SALES_NIJE_ZAINTERESOVAN == "Y" &&
                                !efContext.KONTAKT_HISTORY.Any(h => h.KONTAKT_ID == k.ID && h.STATUS == "prodat")),

                            // --- OTVOREN (nije prodat, nije vraćen, nije nezainteresovan) ---
                            TotalOtvoren = g.Count(k =>
                                k.VRACEN_MARKETINGU != "Y" &&
                                k.SALES_NIJE_ZAINTERESOVAN != "Y" &&
                                !efContext.KONTAKT_HISTORY.Any(h => h.KONTAKT_ID == k.ID && h.STATUS == "prodat"))
                        })
                        .Where(x => x.AgentName != null)
                        .OrderBy(x => x.AgentName)
                        .ToList();

                    model["Sales"] = salesStats;
                }


                ViewBag.DateFrom = dateFrom;
                ViewBag.DateTo = dateTo;

                return View(model);
            }
        }



        [HttpPost]
        public ActionResult Search(Statistika stat)
        {
            var dateTo = (stat.DateTo ?? DateTime.Now).Date.AddHours(23).AddMinutes(59).AddSeconds(59);

            var statistika = efContext.STATISTIKA.Where(s => s.AGENT_ID == stat.SelectedUser && (s.DATETIME >= stat.DateFrom && s.DATETIME <= dateTo)).ToList();

            ViewBag.StatData = statistika;

            string formattedDateFrom = stat.DateFrom?.ToString("dd.MM.yyyy HH:mm");

            string formattedDateTo = dateTo.ToString("dd.MM.yyyy HH:mm");

            ViewBag.dateFrom = formattedDateFrom;

            ViewBag.dateTo = formattedDateTo;

            return View();
        }



    }
}