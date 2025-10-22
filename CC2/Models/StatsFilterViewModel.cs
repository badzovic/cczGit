using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CC2.Models
{
    public class StatsFilterViewModel
    {
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public string Role { get; set; }
    }
}