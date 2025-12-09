using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CC2.Models
{
    public class AgentStatsViewModel
    {
        public string AgentName { get; set; }
        public string Role { get; set; }
        public int TotalCreated { get; set; }
        public int TotalSold { get; set; }
        public int TotalReturned { get; set; }
        public int TotalNotInterested { get; set; }
        public int TotalActive { get; set; }
        public string Status { get; set; }

        public int TotalProdato { get; set; }
        public int TotalPregovori { get; set; }
        public int TotalNijeZainteresovan { get; set; }
        public int TotalOtvoren { get; set; }
        public double Wandlungsquote { get; set; }
    }
}