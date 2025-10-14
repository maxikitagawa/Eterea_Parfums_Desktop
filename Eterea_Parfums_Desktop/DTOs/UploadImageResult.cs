using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eterea_Parfums_Desktop.DTOs
{
    public class UploadImageResult
    {
        public string url { get; set; }
        public string fileName { get; set; }
        public string relativePath { get; set; }
        public long size { get; set; }
    }
}
