using PrakashCRM.Data.Models;
using System;
using System.Collections.Generic;
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
        [Route("Upload")]
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
                var fileName = httpRequest.Form["FileName"];

                if (string.IsNullOrWhiteSpace(lotNo))
                {
                    return BadRequest("lotNo must be provided in the form data.");
                }

                if (string.IsNullOrWhiteSpace(itemNo))
                {
                    return BadRequest("itemNo must be provided in the form data.");
                }

                if (string.IsNullOrWhiteSpace(fileName))
                {
                    return BadRequest("FileName must be provided in the form data.");
                }

                var responses = new List<FileUploadResponse>();

                for (var index = 0; index < httpRequest.Files.Count; index++)
                {
                    var currentFile = httpRequest.Files[index];

                    if (currentFile == null || currentFile.ContentLength == 0)
                    {
                        return BadRequest("One or more provided files are empty.");
                    }

                    byte[] bytes;
                    using (var ms = new MemoryStream())
                    {
                        currentFile.InputStream.CopyTo(ms);
                        bytes = ms.ToArray();
                    }

                    var base64 = Convert.ToBase64String(bytes);
                    var responsesFIleName = Path.GetFileName(fileName);
                    var resolvedExtension = Path.GetExtension(currentFile.FileName);
                    if (string.IsNullOrWhiteSpace(resolvedExtension))
                    {
                        resolvedExtension = Path.GetExtension(responsesFIleName);
                    }

                    SaveAttachmentFile(bytes, lotNo, itemNo, responsesFIleName);

                    responses.Add(new FileUploadResponse
                    {
                        FileName = Path.GetFileNameWithoutExtension(responsesFIleName),
                        FileExtension = resolvedExtension,
                        ContentType = currentFile.ContentType,
                        Size = currentFile.ContentLength,
                        Base64 = base64,
                        LotNo = lotNo,
                        ItemNo = itemNo,
                        IsUploaded = true
                    });
                }

                return Ok(responses);
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

        private static void SaveAttachmentFile(byte[] fileBytes, string lotNo, string itemNo, string fileName)
        {
            var rootPath = HttpContext.Current.Server.MapPath("~/GRNDocumentAttachment");
            var safeItemNo = SanitizePathSegment(itemNo);
            var safeLotNo = SanitizePathSegment(lotNo);
            var safeFileName = SanitizeFileName(fileName);

            var targetDirectory = Path.Combine(rootPath, safeItemNo, safeLotNo);
            Directory.CreateDirectory(targetDirectory);

            var targetFilePath = Path.Combine(targetDirectory, safeFileName);
            File.WriteAllBytes(targetFilePath, fileBytes);
        }

        private static string SanitizePathSegment(string value)
        {
            var input = string.IsNullOrWhiteSpace(value) ? "blank" : value.Trim();

            foreach (var invalidChar in Path.GetInvalidFileNameChars())
            {
                input = input.Replace(invalidChar, '_');
            }

            return input;
        }

        private static string SanitizeFileName(string value)
        {
            var input = string.IsNullOrWhiteSpace(value) ? "attachment" : Path.GetFileName(value.Trim());

            foreach (var invalidChar in Path.GetInvalidFileNameChars())
            {
                input = input.Replace(invalidChar, '_');
            }

            return input;
        }
    }
 }