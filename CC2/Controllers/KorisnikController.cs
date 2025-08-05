using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using CC2.Models;
using DataAccess;

namespace CC2.Controllers
{
    public class KorisnikController : Controller
    {
        private CCEntities efContext = new CCEntities();

        // GET: Korisnik
        public ActionResult Index()
        {
            var successMessage = TempData["Success"] as string;

            if (!string.IsNullOrEmpty(successMessage))
            {
                ViewBag.SuccessMessage = successMessage;
            }

            // Filtriraj samo aktivne korisnike (Active == "Y")
            var users = efContext.AspNetUsers
                .Where(u => u.Active != "N")
                .Select(u => new Korisnici
                {
                    UserId = u.Id,
                    UserName = u.UserName,
                    Email = u.Email,
                    UserRole = efContext.AspNetUserRoles
                        .Where(r => r.UserId == u.Id)
                        .Join(efContext.AspNetRoles, ur => ur.RoleId, r => r.Id, (ur, r) => r.Name)
                        .FirstOrDefault()
                })
                .ToList();

            return View(users);
        }

        // GET: Korisnik/Delete/5
        public ActionResult Delete(string id)
        {
            var user = efContext.AspNetUsers.Find(id);

            if (user != null)
            {
                user.Active = "N"; // Postavi Active na "N"
                efContext.SaveChanges(); // Sačuvaj promene u bazi

                TempData["SuccessMessage"] = "Korisnik uspješno deaktiviran.";
            }
            else
            {
                TempData["NoSuccessMessage"] = "Korisnik nije pronađen.";
            }

            return RedirectToAction("Index", "Korisnik");
        }
    }
}
