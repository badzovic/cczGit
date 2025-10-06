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
    public class NijedobijenController : BaseController
    {
        CCEntities efContext = new CCEntities();

              
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
                                 && k.NIJE_DOBIJEN == "Y"
                                 && k.U_PREGOVORIMA != "Y"
                                 && k.PRODAT != "Y")
                        .OrderByDescending(k => k.ID)
                );
            }
            else if (user.IsInRole("adminmarketing"))
            {
                pregled.kontaktiAdminMarketing = GetKontaktiSaTerminima(
                    efContext.CC_KONTAKTI
                        .Where(k => k.TRENUTNO_KOD_ID == userId
                                 && k.NIJE_DOBIJEN == "Y"
                                 && k.U_PREGOVORIMA != "Y"
                                 && k.PRODAT != "Y")
                        .OrderByDescending(k => k.DATETIME_UPDATED)
                );
            }
            else if (user.IsInRole("adminsales"))
            {
                pregled.kontaktiSalesAdmin = GetKontaktiSaTerminima(
                    efContext.CC_KONTAKTI
                        .Where(k => k.TRENUTNO_KOD_ID == userId
                                 && k.NIJE_DOBIJEN == "Y"
                                 && k.U_PREGOVORIMA != "Y"
                                 && k.PRODAT != "Y")
                        .OrderByDescending(k => k.ID)
                );
            }
            else if (user.IsInRole("sales"))
            {
                pregled.kontaktiSales = GetKontaktiSaTerminima(
                    efContext.CC_KONTAKTI
                        .Where(k => k.TRENUTNO_KOD_ID == userId
                                 && k.NIJE_DOBIJEN == "Y"
                                 && k.U_PREGOVORIMA != "Y"
                                 && k.PRODAT != "Y")
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
    }
}