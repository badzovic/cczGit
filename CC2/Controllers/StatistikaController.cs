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
    public class StatistikaController : Controller
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

        [HttpPost]
        public ActionResult Search(Statistika stat)
        {
            var dateTo = (stat.DateTo ?? DateTime.Now).Date.AddHours(23).AddMinutes(59).AddSeconds(59);

            var statistika = efContext.STATISTIKA.Where(s => s.AGENT_ID == stat.SelectedUser && (s.DATETIME >= stat.DateFrom && s.DATETIME <= dateTo )).ToList();

            ViewBag.StatData = statistika;

            string formattedDateFrom = stat.DateFrom?.ToString("dd.MM.yyyy HH:mm");

            string formattedDateTo = dateTo.ToString("dd.MM.yyyy HH:mm");

            ViewBag.dateFrom = formattedDateFrom;

            ViewBag.dateTo = formattedDateTo;

            return View();
        }
    }
}