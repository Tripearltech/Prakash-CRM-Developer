using PrakashCRM.Data.Models;
using System;
using System.IO;
using System.Net;
using System.Web;
using System.Web.Http;

namespace PrakashCRM.Service.Controllers
{
    [RoutePrefix("api/GRNDocumentAttachment")]
    public class GRNDocumentAttachmentController : ApiController
    {
        [HttpPost]
        [Route("upload")]
        public IHttpActionResult Upload()
        {
            try
            {
                var httpRequest = HttpContext.Current.Request;

                if (httpRequest.Files.Count == 0)
                {
                    return BadRequest("A file must be provided in the request.");
                }

                var file = httpRequest.Files[0];

                if (file == null || file.ContentLength == 0)
                {
                    return BadRequest("The provided file is empty.");
                }

                var lotNo = httpRequest.Form["lotNo"];
                var itemNo = httpRequest.Form["itemNo"];

                if (string.IsNullOrWhiteSpace(lotNo))
                {
                    return BadRequest("lotNo must be provided in the form data.");
                }

                if (string.IsNullOrWhiteSpace(itemNo))
                {
                    return BadRequest("itemNo must be provided in the form data.");
                }

                byte[] bytes;
                using (var ms = new MemoryStream())
                {
                    file.InputStream.CopyTo(ms);
                    bytes = ms.ToArray();
                }

                var base64 = Convert.ToBase64String(bytes);

                var response = new FileUploadResponse
                {
                    FileName = Path.GetFileNameWithoutExtension(file.FileName),
                    FileExtension = Path.GetExtension(file.FileName),
                    ContentType = file.ContentType,
                    Size = file.ContentLength,
                    Base64 = base64,
                    LotNo = lotNo,
                    ItemNo = itemNo
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.InternalServerError, new
                {
                    error = "An error occurred while processing the file.",
                    details = ex.Message
                });
            }
        }
    }
 }