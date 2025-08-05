using DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CC2.Models
{
    public class UploadedFile
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string FilePath { get; set; }
        public List<UPLOADEDFILES> sviFajlovi { get; set; }

    }
}