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
    public class NijeZainteresovanController : Controller
    {
        CCEntities efContext = new CCEntities();

        // GET: NijeZainteresovan
        [Authorize]
        public ActionResult Index()
        {
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


            if (user.IsInRole("adminsales"))
            {
                pregled.nijeZainteresovanSales = efContext.CC_KONTAKTI
               .Where(k => k.TRENUTNO_GRUPA_ID == "5" && k.SALES_NIJE_ZAINTERESOVAN == "Y").OrderByDescending(k => k.DATETIME_UPDATED)
               .ToList();
            }

            return View(pregled);
        }
    }
}