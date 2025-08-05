using DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using NLog;
using CC2.Models;
using System.IO;
using System.Data.Entity;

namespace CC2.Controllers
{
    public class NovostiController : Controller
    {
        CCEntities efContext = new CCEntities();

        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        // GET: Novosti
        public ActionResult Index()
        {

            UploadedFile files = new UploadedFile();

            var dokumenti = efContext.UPLOADEDFILES.OrderByDescending(x => x.ID).ToList();

            files.sviFajlovi = dokumenti;

            return View(files);
        }

        [HttpGet]
        public ActionResult dodaj()
        {
            UploadedFile uploadedFileInfo = new UploadedFile();

            return View(uploadedFileInfo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Dodaj(UploadedFile uploadedFileInfo, HttpPostedFileBase pdfFile)
        {
            if (ModelState.IsValid)
            {
                // Process the uploaded file as before
                if (pdfFile != null && pdfFile.ContentType == "application/pdf")
                {
                 

                    var uploadPath = Server.MapPath("~/Content/UploadedFiles");
                    var fileName = Path.GetFileName(pdfFile.FileName);
                    var fullPath = Path.Combine(uploadPath, fileName);
                    var relativePath = $"/Content/UploadedFiles/{fileName}"; // This is what you should save


                    if (!Directory.Exists(uploadPath))
                    {
                        Directory.CreateDirectory(uploadPath);
                    }

                    pdfFile.SaveAs(fullPath);

                    // Assuming UploadedFile has a FilePath property to store the path
                    uploadedFileInfo.FilePath = relativePath;

                    UPLOADEDFILES files = new UPLOADEDFILES();

                    using (var transaction = efContext.Database.BeginTransaction())
                    {
                        try
                        {
                            files.NAME = uploadedFileInfo.Name;
                            files.FILEPATH = uploadedFileInfo.FilePath;
                            efContext.UPLOADEDFILES.Add(files);
                            int results = efContext.SaveChanges();
                            transaction.Commit();                         
                        }
                        catch (Exception ex)
                        {

                        }
                    }
                }

                else
                {
                    ViewBag.Message = "Invalid file format.";
                }
            }

            TempData["SuccessMessage"] = "Uspješno ste dodali novi file!";
            //   return View(uploadedFileInfo);
            return RedirectToAction("Index", "Novosti");
        }

        [HttpPost]
        public ActionResult DeleteFajl(int id)
        {
            var fajl = efContext.UPLOADEDFILES.FirstOrDefault(f => f.ID == id);
            if (fajl != null)
            {
                efContext.UPLOADEDFILES.Remove(fajl);
                efContext.SaveChanges();
                TempData["Success"] = "Fajl je uspješno obrisan.";
            }
            else
            {
                TempData["Error"] = "Fajl nije pronađen.";
            }

            return RedirectToAction("Index"); 
        }
    }

}