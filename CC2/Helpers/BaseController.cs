using System;
using System.Linq;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using CC2.Models;
using DataAccess;

namespace CC2.Helpers
{
    public class BaseController : Controller
    {
        protected CCEntities efContext = new CCEntities();


        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);

            if (User.Identity.IsAuthenticated)
            {
                var userId = User.Identity.GetUserId();
                var user = HttpContext.User;

             
                // 🔹 Nije dobijen
                ViewBag.CountNijeDobijen = efContext.CC_KONTAKTI.Count(k =>
                    k.TRENUTNO_KOD_ID == userId &&
                    k.NIJE_DOBIJEN == "Y" &&
                    k.VRACEN_MARKETINGU != "Y" &&
                    k.PRODAT != "Y");

                // 🔹 Prebačeni (vraceni marketingu)
                ViewBag.CountVraceni = efContext.CC_KONTAKTI.Count(k =>
                    k.TRENUTNO_KOD_ID == userId &&
                    k.VRACEN_MARKETINGU == "Y" &&
                    k.NIJE_DOBIJEN != "Y" &&
                    k.PRODAT != "Y");

            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && efContext != null)
            {
                efContext.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
