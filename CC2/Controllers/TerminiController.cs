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
    public class TerminiController : Controller
    {
        CCEntities efContext = new CCEntities();

        // GET: Termini
        [Authorize]
        public ActionResult Index()
        {
            Dashboard list = new Dashboard();

            var datum = DateTime.Now;

            var user = HttpContext.User;
            var username = user.Identity.Name;
            var userId = User.Identity.GetUserId();



            //var termini = efContext.TERMINI
            //    .Where(x => x.USER_ID == userId && x.DATUM >= datum)
            //    .ToList() 
            //    .Where(ter =>
            //        efContext.CC_KONTAKTI
            //        .Any(kontakt => kontakt.ID == ter.KONTAKT_ID && kontakt.SALES_NIJE_ZAINTERESOVAN != "Y"))
            //    .OrderBy(x => x.DATUM)
            //    .ToList();


            var initialTermini = efContext.TERMINI
                .Where(x => x.USER_ID == userId && x.DATUM >= datum)
                .ToList();

            var filteredTermini = initialTermini
                .Where(ter => efContext.CC_KONTAKTI
                    .Any(kontakt => kontakt.ID == ter.KONTAKT_ID && kontakt.SALES_NIJE_ZAINTERESOVAN != "Y" && kontakt.PRODAT != "Y"))
                .ToList();

            var distinctTermini = filteredTermini
                .GroupBy(ter => ter.KONTAKT_ID)
                .Select(g => g.OrderByDescending(ter => ter.DATUM).FirstOrDefault())
                .OrderBy(ter => ter.DATUM)
                .ToList();




            list.termini = distinctTermini ?? null;

            return View(list);
        }
    }
}