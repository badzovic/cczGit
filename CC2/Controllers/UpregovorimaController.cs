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
    public class UpregovorimaController : Controller
    {
        CCEntities efContext = new CCEntities();

        // GET: Upregovorima
        [Authorize]
        public ActionResult Index()
        {
            Pregled pregled = new Pregled();
            var user = HttpContext.User;
            var username = user.Identity.Name;
            var userId = User.Identity.GetUserId();

            if (user.IsInRole("sales"))
            {
                pregled.prodato = efContext.CC_KONTAKTI
               .Where(k => k.TRENUTNO_GRUPA_ID == "3" && k.U_PREGOVORIMA == "Y" && k.NIJE_DOBIJEN != "Y" && k.TRENUTNO_KOD_ID == userId).OrderByDescending(k => k.ID)
               .ToList();
            }

            return View(pregled); 
        }
    }
}