using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using PrakashCRM.Data.Models;

namespace PrakashCRM.Service.Controllers
{
    [RoutePrefix("api/WhatsappAttachment")]
    public class WhatsappAttachmentController : ApiController
    {
        [HttpPost]
        [Route("Upload")]
        public IHttpActionResult UploadAttachment([FromBody] WhatsappAttachmentRequest model)
        {
            var response = new WhatsappAttachmentResponse();

            try
            {
                // 🔹 Validation
                if (model == null || string.IsNullOrEmpty(model.Base64))
                {
                    response.Status = false;
                    response.Message = "Base64 is required.";
                    return Content(HttpStatusCode.BadRequest, response);
                }

                if (string.IsNullOrEmpty(model.Extension) || model.Extension.ToLower() != "pdf")
                {
                    response.Status = false;
                    response.Message = "Only PDF file is allowed.";
                    return Content(HttpStatusCode.BadRequest, response);
                }

                if (string.IsNullOrEmpty(model.File_Name))
                {
                    response.Status = false;
                    response.Message = "FileName is required.";
                    return Content(HttpStatusCode.BadRequest, response);
                }

                // 🔹 Remove Base64 header
                string base64Data = model.Base64.Contains(",")
                    ? model.Base64.Split(',')[1]
                    : model.Base64;

                byte[] fileBytes;
                try
                {
                    fileBytes = Convert.FromBase64String(base64Data);
                }
                catch
                {
                    response.Status = false;
                    response.Message = "Invalid Base64 string.";
                    return Content(HttpStatusCode.BadRequest, response);
                }

                // 🔹 Folder Path
                string folderPath = HttpContext.Current.Server.MapPath("~/WhatsApp/");
                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);

                // 🔹 Same File Name
                string fileName = model.File_Name.EndsWith(".pdf")
                    ? model.File_Name
                    : model.File_Name + ".pdf";

                string fullPath = Path.Combine(folderPath, fileName);

                // 🔹 Check file exists or not
                bool isReplaced = File.Exists(fullPath);

                // 🔹 Save / Replace file
                File.WriteAllBytes(fullPath, fileBytes);

                // 🔹 Generate URL
                string domain = HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority);
                string fileUrl = domain + "/WhatsApp/" + fileName;

                response.Status = true;
                response.FileUrl = fileUrl;

                // 🔹 Message based on condition
                response.Message = isReplaced
                    ? "File uploaded successfully (replaced if existed)."
                    : "File uploaded successfully.";

                return Ok(response);
            }
            catch (Exception ex)
            {
                response.Status = false;
                response.Message = ex.Message;
                return Content(HttpStatusCode.InternalServerError, response);
            }
        }


    }
}
