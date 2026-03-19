using System.Collections.Generic;
namespace PrakashCRM.Data.Models
{
    public class WhatsappAttachmentRequest
    {
        public string Base64 { get; set; }
        public string Extension { get; set; }
        public string File_Name { get; set; }
    }

    public class WhatsappAttachmentResponse
    {
        public bool Status { get; set; }
        public string Message { get; set; }
        public string FileUrl { get; set; }
    }
}