using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrakashCRM.Data.Models
{
   public class FileUploadResponse
    {
        public string FileName { get; set; } = string.Empty;

        public string FileExtension { get; set; } = string.Empty;

        public string ContentType { get; set; } = string.Empty;

        [JsonIgnore]
        public long? Size { get; set; }

        public string Base64 { get; set; } = string.Empty;

        public string LotNo { get; set; } = string.Empty;

        public string ItemNo { get; set; } = string.Empty;
    }
}
