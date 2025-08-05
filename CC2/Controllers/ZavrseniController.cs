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
    public class ZavrseniController : Controller
    {
        CCEntities efContext = new CCEntities();

        // GET: Zavrseni
        [Authorize]
        public ActionResult Index()
        {
            Pregled pregled = new Pregled();
            var user = HttpContext.User;
            var username = user.Identity.Name;
            var userId = User.Identity.GetUserId();

            if (user.IsInRole("adminsales"))
            {
                pregled.prodato = efContext.CC_KONTAKTI
               .Where(k => k.TRENUTNO_GRUPA_ID == "5" && k.PRODAT == "Y").OrderByDescending(k => k.DATETIME_UPDATED)
               .ToList();
            }
           
            return View(pregled);
         
        }
    }
}