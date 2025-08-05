using DataAccess;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Xml.Linq;
using System.Web.Mvc;

namespace CC2.Models
{
    public class Statistika
    {
        [Display(Name = "Odaberite agenta")]
        public string SelectedUser { get; set; }

        public List<SelectListItem> sviKorisnici { get; set; }

        // New properties for date range
        [Display(Name = "Datum od")]
        [DataType(DataType.Date)]
        public DateTime? DateFrom { get; set; }

        [Display(Name = "Datum do")]
        [DataType(DataType.Date)]
        public DateTime? DateTo { get; set; }
    }
}