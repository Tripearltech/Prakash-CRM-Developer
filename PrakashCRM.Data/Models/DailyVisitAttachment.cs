using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrakashCRM.Data.Models
{
    public class FileUploadResponse
    {
        public int Table_ID { get; set; }

        public string AttachmentId { get; set; } = string.Empty;

        public string FileName { get; set; } = string.Empty;

        public string FileExtension { get; set; } = string.Empty;

        public string ContentType { get; set; } = string.Empty;

        [JsonIgnore]
        public long? Size { get; set; }

        public string Base64 { get; set; } = string.Empty;

        public string LotNo { get; set; } = string.Empty;

        public string ItemNo { get; set; } = string.Empty;

        public JToken DocAttachmentJson { get; set; } = new JArray();

        public bool IsUploaded { get; set; }
    }

    public class DocAttachmentInfo
    {
        [JsonIgnore]
        public string AttachmentId { get; set; } = string.Empty;

        public string FileName { get; set; } = string.Empty;

        public string FileExtension { get; set; } = string.Empty;

        public string ContentType { get; set; } = string.Empty;

        public string Base64 { get; set; } = string.Empty;

        public string LotNo { get; set; } = string.Empty;

        public string ItemNo { get; set; } = string.Empty;

        public bool IsUploaded { get; set; }
    }

    public class LotNoInformationCard
    {
        public string Item_No { get; set; }
        public string Lot_No { get; set; }
        public string Variant_Code { get; set; }
        public string DocAttachmentJson { get; set; } = string.Empty;
        public bool IsTrackingDocumentAttached { get; set; }
    }

    public class lotNoInformantionResponse
    {
        public bool IsTrackingDocumentAttached { get; set; }
    }

    public class BusinessCentralDocumentAttachment
    {
        public int ID { get; set; }
        public int Table_ID { get; set; }
        public string No { get; set; } = string.Empty;
        public string Document_Type { get; set; } = string.Empty;
        public int Line_No { get; set; }
        public string Item_No { get; set; } = string.Empty;
        public string File_Name { get; set; } = string.Empty;
        public string File_Extension { get; set; } = string.Empty;
        public string Base64Text { get; set; } = string.Empty;
        public string Attached_Date { get; set; } = string.Empty;

        public bool ShouldSerializeID() => false;

        public bool ShouldSerializeDocument_Type() => false;

        public bool ShouldSerializeLine_No() => false;

        public bool ShouldSerializeAttached_Date() => false;
    }

    public class BusinessCentralDocumentAttachmentWriteRequest
    {
        public int Table_ID { get; set; }
        public string No { get; set; } = string.Empty;
        public string Item_No { get; set; } = string.Empty;
        public string File_Name { get; set; } = string.Empty;
        public string File_Extension { get; set; } = string.Empty;
        public string Base64Text { get; set; } = string.Empty;
    }

    public class GRNDocumentAttachmentSyncRequest
    {
        public int Table_ID { get; set; } = 6505;
        public string ItemNo { get; set; } = string.Empty;
        public string LotNo { get; set; } = string.Empty;
        public List<FileUploadResponse> Attachments { get; set; } = new List<FileUploadResponse>();
    }

    public class GRNDocumentAttachmentDeleteRequest
    {
        public int Table_ID { get; set; } = 6505;
        public string ItemNo { get; set; } = string.Empty;
        public string LotNo { get; set; } = string.Empty;
        public int ID { get; set; }
        public string DocumentType { get; set; } = string.Empty;
        public int LineNo { get; set; }
        public string AttachmentId { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
    }

}
