using CC2.Models;
using DataAccess;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using NLog;
using System.Configuration;

namespace CC2.Controllers
{
    public class KontaktiController : Controller
    {
        CCEntities efContext = new CCEntities();

        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        // GET: Kontakti

        [HttpGet]
        public ActionResult Index()
        {
            Kontakti kontakt = new Kontakti();
            return View(kontakt);
        }

        [Authorize]
        [HttpGet]
        public ActionResult Kalendar(int? id)
        {
            using (var efContext = new CCEntities())
            {
                //var kontakt = efContext.CC_KONTAKTI.Find(id);

                //if (kontakt == null)
                //    return HttpNotFound();
                var model = new Kontakti();



                var loggedUserId = User.Identity.GetUserId();
                var loggedUserRoleId = efContext.AspNetUserRoles
                    .Where(r => r.UserId == loggedUserId)
                    .Select(r => r.RoleId)
                    .FirstOrDefault();

                if (id.HasValue)
                {
                    var kontakt = efContext.CC_KONTAKTI.Find(id.Value);
                    if (kontakt == null)
                        return HttpNotFound();

                    model.Id = kontakt.ID;
                    model.firma = kontakt.FIRMA;
                    model.brojkartica = kontakt.BROJ_KARTICA;
                }
                else
                {
                    model.Id = 0; // ili ostavi null ako tvoj View to podržava
                    model.firma = "Svi kontakti";
                }


              
                var terminiRawQuery = from t in efContext.TERMINI
                                      join ur in efContext.AspNetUserRoles on t.USER_ID.ToString() equals ur.UserId
                                      join u in efContext.AspNetUsers on ur.UserId equals u.Id
                                      join kTemp in efContext.CC_KONTAKTI on t.KONTAKT_ID equals kTemp.ID into kontaktJoin
                                      from k in kontaktJoin.DefaultIfEmpty()
                                      select new
                                      {
                                          id = t.ID,
                                          DATUM = t.DATUM,
                                          END = t.DATUM_KRAJA,
                                          EMAIL = u.Email,
                                          NAZIV = t.NAZIV,
                                          KONTAKT_ID = t.KONTAKT_ID,
                                          USER_ID = t.USER_ID.ToString(),
                                          ROLE_ID = ur.RoleId,
                                          brojKartica = k != null ? k.BROJ_KARTICA.ToString() : "0"
                                      };
                // FILTRIRANJE PO ULOGAMA
                if (loggedUserRoleId == "3") // SALES
                {
                    // vidi samo svoje termine
                    terminiRawQuery = terminiRawQuery.Where(t => t.USER_ID == loggedUserId);
                }
                else if (loggedUserRoleId == "2") // MARKETING
                {
                }
                else
                {
                    
                }

              

                var terminiRaw = terminiRawQuery.ToList();

                var termini = terminiRaw
                    .Select(t => new Termin
                    {
                        ID = t.id,
                        DATUM = t.DATUM,                      
                        KRAJ = t.END,
                        NAZIV = efContext.CC_KONTAKTI
                                  .Where(k => k.ID == t.KONTAKT_ID)
                                  .Select(k => k.FIRMA)
                                  .FirstOrDefault() + " - " + t.EMAIL,
                        Boja = GenerateColor(t.EMAIL),
                        UserId = t.USER_ID,
                        KONTAKT_ID = t.KONTAKT_ID,
                        BrojKartica = t.brojKartica,
                    }).ToList();

                model.Termini = termini;
                ViewBag.UserId = loggedUserId;
                ViewBag.RoleId = loggedUserRoleId;
                return View(model);
            }
        }

        [HttpPost]
        public JsonResult AzurirajTermin(int terminId, DateTime noviStart, DateTime? noviEnd)
        {
            try
            {
                using (var ef = new CCEntities())
                {
                    var termin = ef.TERMINI.FirstOrDefault(t => t.ID == terminId);
                    if (termin == null)
                        return Json(new { success = false, message = "Termin nije pronađen." });

                    termin.DATUM = noviStart;
                    termin.DATUM_KRAJA = noviEnd;
                    ef.SaveChanges();

                    return Json(new { success = true });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult ObrisiTermin(int id)
        {
            try
            {
                using (var ef = new CCEntities())
                {
                    var termin = ef.TERMINI.FirstOrDefault(t => t.ID == id);
                    if (termin == null)
                        return Json(new { success = false, message = "Termin nije pronađen." });

                    ef.TERMINI.Remove(termin);
                    ef.SaveChanges();

                    return Json(new { success = true });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }



        [Authorize]
        [HttpGet]
        public ActionResult Create()
        {
            var kontakt = new Kontakti();

            using (var efContext = new CCEntities())
            {
                var terminiRaw = (from t in efContext.TERMINI
                                  join ur in efContext.AspNetUserRoles on t.USER_ID.ToString() equals ur.UserId
                                  join u in efContext.AspNetUsers on ur.UserId equals u.Id
                                  where ur.RoleId == "3"
                                  select new
                                  {
                                      DATUM = t.DATUM,
                                      EMAIL = u.Email
                                  }).ToList();

                var termini = terminiRaw
                .Select(t => new Termin
                {
                    DATUM = t.DATUM,
                    NAZIV = t.EMAIL,
                    Boja = GenerateColor(t.EMAIL)
                }).ToList();
                kontakt.Termini = termini;
            }

            return View(kontakt);
        }

      
        public ActionResult CheckNumber(string prefix, string broj, string broj2)
        {
            bool exists = false;
            bool existsStat = false;
            var user = HttpContext.User;
            var username = user.Identity.Name;
            var userId = User.Identity.GetUserId();

            try
            {
                // Query the database using Entity Framework
                if (broj != "" && broj2 != "")
                {
                    exists = efContext.CC_KONTAKTI.Any(c =>
                    (c.PREFIX == prefix && (c.BROJ == broj || c.BROJ2 == broj)) ||
                    (c.PREFIX == prefix && (c.BROJ == broj2 || c.BROJ2 == broj2)));
                }
                else if (broj != "")
                {
                    exists = efContext.CC_KONTAKTI.Any(c => c.PREFIX == prefix && c.BROJ == broj || c.PREFIX == prefix && c.BROJ2 == broj);
                }
                else if (broj2 != "")
                {
                    exists = efContext.CC_KONTAKTI.Any(c => c.PREFIX == prefix && c.BROJ == broj2 || c.PREFIX == prefix && c.BROJ2 == broj2);
                }



              

                STATISTIKA stat = new STATISTIKA();

                existsStat = efContext.STATISTIKA.Any(c => c.AGENT_ID == userId && c.BROJ == broj || c.AGENT_ID == userId && c.BROJ == broj2);

                if (!existsStat)
                {
                    using (var transaction = efContext.Database.BeginTransaction())
                    {
                        try
                        {
                            
                            stat.AGENT_ID = userId; 
                            if (broj != "" && broj2 != "")
                            {
                                stat.BROJ = prefix + broj;
                            }
                            else if (broj != "")
                            {
                                stat.BROJ = prefix + broj;
                            }
                            else if (broj2 != "")
                            {
                                stat.BROJ = prefix + broj;
                            }
                            stat.PROVJERIO = "Y";
                            stat.DATETIME = DateTime.Now;

                            efContext.STATISTIKA.Add(stat);
                            int results = efContext.SaveChanges();
                            transaction.Commit();
                        }
                        catch (Exception ex)
                        {
                            _logger.Error("Greska na metodi check number kod upisa statistike." + " " + ex.Message);
                        }
                    }

                }

                if (!exists)
                {
                    var response = new { success = true, exists = false };
                    return Json(response, JsonRequestBehavior.AllowGet);
                }

                return Json(new { success = true, exists = true }, JsonRequestBehavior.AllowGet);

                // Return a JSON response indicating whether the record exists


            }
            catch (Exception ex)
            {
                _logger.Error("Greska na metodi CheckNumber get." + " " + ex.Message);

                var errorResponse = new { success = false, error = ex.Message };
                return Json(errorResponse, JsonRequestBehavior.AllowGet);
            }
        }
        [Authorize]
        [HttpPost]
        public ActionResult Create(Kontakti kontakt, string submitButton, string TerminDatum, string TerminVrijemeOd, string TerminVrijemeDo)
        {

            CC_KONTAKTI efKontakti = new CC_KONTAKTI();
            TERMINI termin = new TERMINI();

            var user = HttpContext.User;
            var username = user.Identity.Name;
            var userId = User.Identity.GetUserId();

            var roleId = efContext.AspNetUserRoles
            .Where(r => r.UserId == userId)
            .Select(r => r.RoleId)
            .FirstOrDefault();


            if (roleId == "3") 
            {
                using (var transaction = efContext.Database.BeginTransaction())
                {
                    try
                    {
                        efKontakti.KREIRAO_ID = userId;
                        efKontakti.TRENUTNO_KOD_ID = userId;
                        efKontakti.TRENUTNO_GRUPA_ID = "3";
                        efKontakti.FINALIZIRAO_ID = userId;
                        efKontakti.GENDER = kontakt.gender;
                        efKontakti.IME = kontakt.ime;
                        efKontakti.PREZIME = kontakt.prezime;
                        efKontakti.ADRESA = kontakt.adresa;
                        efKontakti.PLZ = kontakt.plz;
                        efKontakti.DRZAVA = kontakt.drzava;
                        efKontakti.GRAD = kontakt.grad;
                        efKontakti.PREFIX = kontakt.prefix;
                        efKontakti.BROJ = kontakt.broj;
                        efKontakti.BROJ2 = kontakt.broj2;
                        efKontakti.EMAIL = kontakt.email;
                        efKontakti.FAX = kontakt.fax;
                        efKontakti.BEI = kontakt.bei;
                        efKontakti.DOSTUPNOST = kontakt.dostupnost;
                        efKontakti.MOGUCA_INVESTICIJA = kontakt.investicija;
                        efKontakti.KOMENTAR = kontakt.komentar;
                        efKontakti.DATETIME_CREATED = DateTime.Now;
                        efKontakti.AKTIVAN = "Y";
                        efKontakti.FIRMA = kontakt.firma;
                        efKontakti.BROJ_KARTICA = kontakt.brojkartica;

                        efContext.CC_KONTAKTI.Add(efKontakti);
                        efContext.SaveChanges();


                        if (!string.IsNullOrWhiteSpace(TerminDatum) &&
                        !string.IsNullOrWhiteSpace(TerminVrijemeOd) &&
                        !string.IsNullOrWhiteSpace(TerminVrijemeDo))
                        {
                            var start = DateTime.Parse($"{TerminDatum} {TerminVrijemeOd}");
                            var end = DateTime.Parse($"{TerminDatum} {TerminVrijemeDo}");

                            termin.USER_ID = userId;
                            termin.DATUM = start;
                            termin.DATUM_KRAJA = end;
                            termin.NAZIV = kontakt.ime + " " + kontakt.prezime;
                            termin.KONTAKT_ID = efKontakti.ID;


                            efContext.TERMINI.Add(termin);
                            efContext.SaveChanges();

                            efKontakti.TERMIN_ID = termin.ID;
                            efContext.SaveChanges();
                        }
                        else
                        {
                            efKontakti.TERMIN_ID = null;
                            efContext.SaveChanges();
                        }

                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {

                    }
                }
            }
            else
            {
                using (var transaction = efContext.Database.BeginTransaction())
                {
                    try
                    {
                        efKontakti.KREIRAO_ID = userId;
                        efKontakti.TRENUTNO_KOD_ID = userId;
                        efKontakti.TRENUTNO_GRUPA_ID = "2";
                        efKontakti.FINALIZIRAO_ID = userId;
                        efKontakti.GENDER = kontakt.gender;
                        efKontakti.IME = kontakt.ime;
                        efKontakti.PREZIME = kontakt.prezime;
                        efKontakti.ADRESA = kontakt.adresa;
                        efKontakti.PLZ = kontakt.plz;
                        efKontakti.DRZAVA = kontakt.drzava;
                        efKontakti.GRAD = kontakt.grad;
                        efKontakti.PREFIX = kontakt.prefix;
                        efKontakti.BROJ = kontakt.broj;
                        efKontakti.BROJ2 = kontakt.broj2;
                        efKontakti.EMAIL = kontakt.email;
                        efKontakti.FAX = kontakt.fax;
                        efKontakti.BEI = kontakt.bei;
                        efKontakti.DOSTUPNOST = kontakt.dostupnost;
                        efKontakti.MOGUCA_INVESTICIJA = kontakt.investicija;
                        efKontakti.KOMENTAR = kontakt.komentar;
                        efKontakti.DATETIME_CREATED = DateTime.Now;
                        efKontakti.AKTIVAN = "Y";
                        efKontakti.FIRMA = kontakt.firma;
                        efKontakti.BROJ_KARTICA = kontakt.brojkartica;
                        efContext.CC_KONTAKTI.Add(efKontakti);
                        efContext.SaveChanges();

                        if (!string.IsNullOrWhiteSpace(TerminDatum) &&
                        !string.IsNullOrWhiteSpace(TerminVrijemeOd) &&
                        !string.IsNullOrWhiteSpace(TerminVrijemeDo))
                        {
                            var start = DateTime.Parse($"{TerminDatum} {TerminVrijemeOd}");
                            var end = DateTime.Parse($"{TerminDatum} {TerminVrijemeDo}");

                            termin.USER_ID = userId;
                            termin.DATUM = start;
                            termin.DATUM_KRAJA = end;
                            termin.NAZIV = kontakt.ime + " " + kontakt.prezime;
                            termin.KONTAKT_ID = efKontakti.ID;


                            efContext.TERMINI.Add(termin);
                            efContext.SaveChanges();

                            efKontakti.TERMIN_ID = termin.ID;
                            efContext.SaveChanges();
                        }
                        else
                        {
                            efKontakti.TERMIN_ID = null;
                            efContext.SaveChanges();
                        }

                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        _logger.Error("Greska kod upisa u bazu na methodi Create." + " " + ex.Message);

                    }
                }
            }

            TempData["Success"] = "Uspješno ste dodali novi kontakt!";

            return RedirectToAction("Index", "Pregled");
        }
        [HttpPost]
        public JsonResult ZakaziTermin(int kontaktId, DateTime start, DateTime end)
        {
            try
            {
                using (var ef = new CCEntities())
                {
                    // Pronađi trenutno prijavljenog korisnika
                    var username = User.Identity.Name;
                    var user = ef.AspNetUsers.FirstOrDefault(u => u.UserName == username);

                    if (user == null)
                    {
                        return Json(new { success = false, message = "Korisnik nije pronađen." });
                    }

                    // Pronađi sve role korisnika
                    var userRoles = ef.AspNetUserRoles
                                      .Where(r => r.UserId == user.Id)
                                      .Select(r => r.RoleId)
                                      .ToList();

                    // Ako korisnik ima rolu sa ID = 3 (sales)
                    string userIdToSet;
                    string nazivToSet;

                    if (userRoles.Contains("3"))
                    {
                        userIdToSet = user.Id;
                        nazivToSet = username + " " + "uspješno rezevrisao";
                    }
                       
                    else
                    {
                        userIdToSet = "fafaa80b-27db-455e-bf3a-70c483c6ae5f"; // default
                        nazivToSet = "Marketing rezervacija";
                    }
                      
                    var termin = new TERMINI
                    {
                        DATUM = start,
                        DATUM_KRAJA = end,
                        KONTAKT_ID = kontaktId,
                        USER_ID = userIdToSet,
                        NAZIV = nazivToSet
                    };

                    ef.TERMINI.Add(termin);
                    ef.SaveChanges();

                    return Json(new { success = true, id = termin.ID });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }



        [HttpGet]
        public ActionResult editSales(int id)
        {
            var contactToEdit = efContext.CC_KONTAKTI.Find(id);

            if (contactToEdit != null)
            {
                var viewModel = new Kontakti
                {
                    Id = contactToEdit.ID,
                    firma = contactToEdit.FIRMA,
                    brojkartica = contactToEdit.BROJ_KARTICA,
                    ime = contactToEdit.IME,
                    prezime = contactToEdit.PREZIME,
                    gender = contactToEdit.GENDER,
                    adresa = contactToEdit.ADRESA,
                    plz = contactToEdit.PLZ,
                    drzava = contactToEdit.DRZAVA,
                    grad = contactToEdit.GRAD,
                    prefix = contactToEdit.PREFIX,
                    broj = contactToEdit.BROJ,
                    broj2 = contactToEdit.BROJ2,
                    email = contactToEdit.EMAIL,
                    dostupnost = contactToEdit.DOSTUPNOST,
                    investicija = contactToEdit.MOGUCA_INVESTICIJA,
                    komentar = contactToEdit.KOMENTAR,
                    terminId = contactToEdit.TERMIN_ID,
                    vracenMarketingu = contactToEdit.VRACEN_MARKETINGU,
                    vracenSakontrole = contactToEdit.VRACENO_SA_KONTROLE

                };
                if (contactToEdit.TERMIN_ID.HasValue)
                {
                    var termin = efContext.TERMINI.Find(contactToEdit.TERMIN_ID.Value);
                    if (termin != null)
                    {
                        viewModel.terminDate = termin.DATUM;
                        viewModel.terminEndDate = termin.DATUM_KRAJA;
                    }
                }
                return View(viewModel);
            }
            else
            {
                return HttpNotFound();
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult editSales(Kontakti kontakt)
        {


            var user = HttpContext.User;
            var username = user.Identity.Name;
            var userId = User.Identity.GetUserId();
            var efKontaktiToUpdate = efContext.CC_KONTAKTI.Find(kontakt.Id);
            var terminToUpdateId = efKontaktiToUpdate.TERMIN_ID;

            TERMINI terminToUpdate = new TERMINI();

            TERMINI termin = new TERMINI();

            


            using (var transaction = efContext.Database.BeginTransaction())
            {
                try
                {


                    efKontaktiToUpdate.IME = kontakt.ime;
                    efKontaktiToUpdate.FIRMA = kontakt.firma;
                    efKontaktiToUpdate.BROJ_KARTICA = kontakt.brojkartica;
                    efKontaktiToUpdate.PREZIME = kontakt.prezime;
                    efKontaktiToUpdate.ADRESA = kontakt.adresa;
                    efKontaktiToUpdate.PLZ = kontakt.plz;
                    efKontaktiToUpdate.GRAD = kontakt.grad;
                    efKontaktiToUpdate.PREFIX = kontakt.prefix;
                    efKontaktiToUpdate.BROJ = kontakt.broj;
                    efKontaktiToUpdate.BROJ2 = kontakt.broj2;
                    efKontaktiToUpdate.EMAIL = kontakt.email;
                    efKontaktiToUpdate.FAX = kontakt.fax;
                    efKontaktiToUpdate.DOSTUPNOST = kontakt.dostupnost;
                    efKontaktiToUpdate.MOGUCA_INVESTICIJA = kontakt.investicija + " - " + DateTime.Now;
                    if (kontakt.komentar == null || string.IsNullOrWhiteSpace(kontakt.komentar.Trim()))
                    {
                        // Ako je unos potpuno prazan – očisti KOMENTAR u bazi
                        efKontaktiToUpdate.KOMENTAR = null;
                    }
                    else
                    {
                        var noviKomentari = kontakt.komentar
                            .Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries)
                            .Select(line => line.Trim())
                            .Where(line => !string.IsNullOrWhiteSpace(line))
                            .Distinct()
                            .ToList();

                        var stariKomentar = efKontaktiToUpdate.KOMENTAR ?? "";
                        var linijePostojece = stariKomentar.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries).ToList();

                        foreach (var linija in noviKomentari)
                        {
                            // Ne dodaj liniju ako već postoji ista (ignoriramo datum)
                            if (!linijePostojece.Any(x => x.EndsWith(linija)))
                            {
                                var zapis = $"[{DateTime.Now:dd.MM.yyyy. HH:mm}] {linija}";
                                stariKomentar = zapis + "\n" + stariKomentar;
                            }
                        }

                        efKontaktiToUpdate.KOMENTAR = stariKomentar.Trim();
                    }

                    efKontaktiToUpdate.DATETIME_UPDATED = DateTime.Now;
                    efKontaktiToUpdate.AKTIVAN = "Y";
                    if (kontakt.terminDate != DateTime.MinValue)
                    {
                         terminToUpdateId = efKontaktiToUpdate.TERMIN_ID;
                         terminToUpdate = efContext.TERMINI.Find(terminToUpdateId);

                        if (terminToUpdate != null)
                        {
                            // Ažuriraj postojeći termin
                            terminToUpdate.DATUM = kontakt.terminDate;
                            terminToUpdate.DATUM_KRAJA = kontakt.terminEndDate;
                            terminToUpdate.NAZIV = kontakt.ime + " " + kontakt.prezime;
                            terminToUpdate.USER_ID = userId;
                            terminToUpdate.KONTAKT_ID = kontakt.Id;
                        }
                        else
                        {
                            // Kreiraj novi termin
                            var noviTermin = new TERMINI
                            {
                                USER_ID = userId,
                                DATUM = kontakt.terminDate,
                                DATUM_KRAJA = kontakt.terminEndDate,
                                NAZIV = kontakt.ime + " " + kontakt.prezime,
                                KONTAKT_ID = kontakt.Id
                            };

                            efContext.TERMINI.Add(noviTermin);
                            efContext.SaveChanges();

                            efKontaktiToUpdate.TERMIN_ID = noviTermin.ID;
                        }
                    }
                    int results2 = efContext.SaveChanges();
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    _logger.Error("Greska na metodi edtSales upis u bazu." + " " + ex.Message);

                }
            }



            TempData["Success"] = "Uspješno ste izmjenili kontakt!";

            string url = $"/Kontakti/editSales/{kontakt.Id}";

            // Redirect to the URL
            return Redirect(url);

          
        }

        [HttpGet]
        public ActionResult Edit(int id)
        {
            var contactToEdit = efContext.CC_KONTAKTI.Find(id);

            if (contactToEdit != null)
            {
                var viewModel = new Kontakti
                {
                    Id = contactToEdit.ID,
                    firma = contactToEdit.FIRMA,
                    brojkartica = contactToEdit.BROJ_KARTICA,
                    ime = contactToEdit.IME,
                    prezime = contactToEdit.PREZIME,
                    gender = contactToEdit.GENDER,
                    adresa = contactToEdit.ADRESA,
                    plz = contactToEdit.PLZ,
                    drzava = contactToEdit.DRZAVA,
                    grad = contactToEdit.GRAD,
                    prefix = contactToEdit.PREFIX,
                    broj = contactToEdit.BROJ,
                    broj2 = contactToEdit.BROJ2,
                    email = contactToEdit.EMAIL,
                    dostupnost = contactToEdit.DOSTUPNOST,
                    investicija = contactToEdit.MOGUCA_INVESTICIJA,
                    komentar = contactToEdit.KOMENTAR,
                    terminId = contactToEdit.TERMIN_ID,
                    vracenMarketingu = contactToEdit.VRACEN_MARKETINGU,
                    vracenSakontrole = contactToEdit.VRACENO_SA_KONTROLE

                };
                if (contactToEdit.TERMIN_ID.HasValue)
                {
                    var termin = efContext.TERMINI.Find(contactToEdit.TERMIN_ID.Value);
                    if (termin != null)
                    {
                        viewModel.terminDate = termin.DATUM;
                        viewModel.terminEndDate = termin.DATUM_KRAJA;
                    }
                }

                return View(viewModel);
            }
            else
            {
                return HttpNotFound();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Kontakti kontakt)
        {

            var efKontaktiToUpdate = efContext.CC_KONTAKTI.Find(kontakt.Id);

            var terminToUpdateId = efKontaktiToUpdate.TERMIN_ID;

            var terminToUpdate = efContext.TERMINI.Find(terminToUpdateId);

            TERMINI termin = new TERMINI();

            var user = HttpContext.User;
            var username = user.Identity.Name;
            var userId = User.Identity.GetUserId();





            using (var transaction = efContext.Database.BeginTransaction())
            {
                try
                {


                    efKontaktiToUpdate.IME = kontakt.ime;
                    efKontaktiToUpdate.FIRMA = kontakt.firma;
                    efKontaktiToUpdate.BROJ_KARTICA = kontakt.brojkartica;
                    efKontaktiToUpdate.PREZIME = kontakt.prezime;
                    efKontaktiToUpdate.ADRESA = kontakt.adresa;
                    efKontaktiToUpdate.PLZ = kontakt.plz;
                    efKontaktiToUpdate.GRAD = kontakt.grad;
                    efKontaktiToUpdate.PREFIX = kontakt.prefix;
                    efKontaktiToUpdate.BROJ = kontakt.broj;
                    efKontaktiToUpdate.BROJ2 = kontakt.broj2;
                    efKontaktiToUpdate.EMAIL = kontakt.email;
                    efKontaktiToUpdate.FAX = kontakt.fax;
                    efKontaktiToUpdate.DOSTUPNOST = kontakt.dostupnost;
                    efKontaktiToUpdate.MOGUCA_INVESTICIJA = kontakt.investicija;
                    efKontaktiToUpdate.KOMENTAR = kontakt.komentar;
                    efKontaktiToUpdate.DATETIME_UPDATED = DateTime.Now;
                    efKontaktiToUpdate.AKTIVAN = "Y";
                    if (kontakt.terminDate != DateTime.MinValue)
                    {
                        if (terminToUpdate != null)
                        {
                            // Ažuriraj postojeći termin
                            terminToUpdate.DATUM = kontakt.terminDate;
                            terminToUpdate.DATUM_KRAJA = kontakt.terminEndDate; 
                            terminToUpdate.NAZIV = kontakt.ime + " " + kontakt.prezime;
                            terminToUpdate.USER_ID = userId;
                            terminToUpdate.KONTAKT_ID = kontakt.Id;
                        }
                        else
                        {
                            // Kreiraj novi termin
                            var noviTermin = new TERMINI
                            {
                                USER_ID = userId,
                                DATUM = kontakt.terminDate,
                                DATUM_KRAJA = kontakt.terminEndDate, 
                                NAZIV = kontakt.ime + " " + kontakt.prezime,
                                KONTAKT_ID = kontakt.Id
                            };

                            efContext.TERMINI.Add(noviTermin);
                            efContext.SaveChanges();

                            efKontaktiToUpdate.TERMIN_ID = noviTermin.ID;
                        }
                    }


                    int results2 = efContext.SaveChanges();
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    _logger.Error("Greska na metodi Edit." + " " + ex.Message);
                }

            }



            TempData["Success"] = "Uspješno ste izmjenili kontakt!";

            return RedirectToAction("Index", "Pregled");
        }
        private string GenerateColor(string input)
        {
            // Simple hash to color
            int hash = input.GetHashCode();
            Random rand = new Random(hash);
            return $"#{rand.Next(0x1000000):X6}";
        }
    }
}