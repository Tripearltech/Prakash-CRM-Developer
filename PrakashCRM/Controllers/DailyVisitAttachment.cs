using Newtonsoft.Json;
using PrakashCRM.Data.Models;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Presentation;
using Newtonsoft.Json;
using PrakashCRM.Data.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Net;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace PrakashCRM.Controllers
{
        [RoutePrefix("DailyVisitAttachment")]
        public class DailyVisitAttachmentController : Controller
        {
            // GET: GRNDocumentAttacment
            [Route("")]
            public ActionResult Index()
            {
                return View();
            }
            // Daily Visit Upload Attachment Document
            [HttpPost]
            [Route("UploadDocumentAttachment")]
            public async Task<ActionResult> UploadDocumentAttachment(string no, string entryType, string FileName)
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(no))
                    {
                        Response.StatusCode = (int)HttpStatusCode.BadRequest;
                        return Json(new { error = "no is required." });
                    }

                    if (string.IsNullOrWhiteSpace(entryType))
                    {
                        Response.StatusCode = (int)HttpStatusCode.BadRequest;
                        return Json(new { error = "entryType is required." });
                    }

                    if (string.IsNullOrWhiteSpace(FileName))
                    {
                        Response.StatusCode = (int)HttpStatusCode.BadRequest;
                        return Json(new { error = "FileName is required." });
                    }

                    if (Request.Files.Count == 0)
                    {
                        Response.StatusCode = (int)HttpStatusCode.BadRequest;
                        return Json(new { error = "At least one file is required." });
                    }

                    var serviceBaseUrl = ConfigurationManager.AppSettings["ServiceApiUrl"]?.ToString() ?? string.Empty;
                    string apiUrl = BuildServiceApiUrl(serviceBaseUrl, "DocumentAttachment/Upload");
                    HttpPostedFileBase postedFile = null;

                    for (int index = 0; index < Request.Files.Count; index++)
                    {
                        if (Request.Files[index] != null && Request.Files[index].ContentLength > 0)
                        {
                            postedFile = Request.Files[index];
                            break;
                        }
                    }

                    if (postedFile == null)
                    {
                        Response.StatusCode = (int)HttpStatusCode.BadRequest;
                        return Json(new { error = "A valid file is required." });
                    }

                    using (HttpClient client = new HttpClient())
                    using (MultipartFormDataContent multipartContent = new MultipartFormDataContent())
                    {
                        client.Timeout = TimeSpan.FromMinutes(5);
                        client.BaseAddress = new Uri(apiUrl);
                        client.DefaultRequestHeaders.Accept.Clear();
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                        multipartContent.Add(new StringContent(no), "no");
                        multipartContent.Add(new StringContent(entryType), "entryType");
                        multipartContent.Add(new StringContent(FileName), "FileName");

                        if (postedFile.InputStream.CanSeek)
                        {
                            postedFile.InputStream.Position = 0;
                        }

                        StreamContent fileContent = new StreamContent(postedFile.InputStream);
                        fileContent.Headers.ContentType = new MediaTypeHeaderValue(string.IsNullOrWhiteSpace(postedFile.ContentType) ? "application/octet-stream" : postedFile.ContentType);
                        multipartContent.Add(fileContent, "files", Path.GetFileName(postedFile.FileName));
                        HttpResponseMessage response = await client.PostAsync(apiUrl, multipartContent);
                        string responseData = await response.Content.ReadAsStringAsync();
                        if (!response.IsSuccessStatusCode)
                        {
                            Response.StatusCode = (int)response.StatusCode;
                            return Json(new { error = responseData });
                        }
                        return Content(responseData, "application/json");
                    }
                }
                catch (Exception ex)
                {
                    Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    return Json(new
                    {
                        error = "Upload failed while forwarding the document.",
                        details = ex.Message
                    });
                }
            }

            [HttpGet]
            [Route("GetDocumentAttachments")]
            public async Task<ActionResult> GetDocumentAttachments(int tableId, string entryType, string no)
            {
                try
                {
                    if (tableId <= 0)
                    {
                        Response.TrySkipIisCustomErrors = true;
                        Response.StatusCode = (int)HttpStatusCode.BadRequest;
                        return Json(new { error = "tableId is required." }, JsonRequestBehavior.AllowGet);
                    }

                    if (string.IsNullOrWhiteSpace(entryType))
                    {
                        Response.TrySkipIisCustomErrors = true;
                        Response.StatusCode = (int)HttpStatusCode.BadRequest;
                        return Json(new { error = "entryType is required." }, JsonRequestBehavior.AllowGet);
                    }

                    if (string.IsNullOrWhiteSpace(no))
                    {
                        Response.TrySkipIisCustomErrors = true;
                        Response.StatusCode = (int)HttpStatusCode.BadRequest;
                        return Json(new { error = "no is required." }, JsonRequestBehavior.AllowGet);
                    }

                    var serviceBaseUrl = ConfigurationManager.AppSettings["ServiceApiUrl"]?.ToString() ?? string.Empty;
                    string apiUrl = BuildServiceApiUrl(serviceBaseUrl, $"DocumentAttachment/List?tableId={tableId}&entryType={Uri.EscapeDataString(entryType)}&no={Uri.EscapeDataString(no)}");
                    using (HttpClient client = new HttpClient())
                    {
                        client.Timeout = TimeSpan.FromMinutes(5);
                        client.DefaultRequestHeaders.Accept.Clear();
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                        HttpResponseMessage response = await client.GetAsync(apiUrl);
                        string responseData = await response.Content.ReadAsStringAsync();

                        Response.TrySkipIisCustomErrors = true;
                        if (!response.IsSuccessStatusCode)
                        {
                            Response.StatusCode = (int)response.StatusCode;
                            return Content(string.IsNullOrWhiteSpace(responseData) ? JsonConvert.SerializeObject(new { error = "Unable to fetch attachments." }) : responseData, "application/json");
                        }

                        return Content(string.IsNullOrWhiteSpace(responseData) ? "[]" : responseData, "application/json");
                    }
                }
                catch (Exception ex)
                {
                    Response.TrySkipIisCustomErrors = true;
                    Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    return Json(new
                    {
                        error = "Fetch failed while forwarding document attachments.",
                        details = ex.Message
                    }, JsonRequestBehavior.AllowGet);
                }
            }

            [HttpPost]
            [Route("DeleteDocumentAttachment")]
            public async Task<ActionResult> DeleteDocumentAttachment(GRNDocumentAttachmentDeleteRequest request)
            {
                try
                {
                    if (request == null)
                    {
                        Response.StatusCode = (int)HttpStatusCode.BadRequest;
                        return Json(new { error = "Delete request is required." });
                    }

                    var serviceBaseUrl = ConfigurationManager.AppSettings["ServiceApiUrl"]?.ToString() ?? string.Empty;
                    string apiUrl = BuildServiceApiUrl(serviceBaseUrl, "DocumentAttachment/Delete");

                    using (HttpClient client = new HttpClient())
                    {
                        client.Timeout = TimeSpan.FromMinutes(5);
                        client.DefaultRequestHeaders.Accept.Clear();
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        client.DefaultRequestHeaders.TryAddWithoutValidation("If-Match", "*"); // Required for Business Central OData delete

                        string requestJson = JsonConvert.SerializeObject(request);
                        StringContent content = new StringContent(requestJson, Encoding.UTF8, "application/json");
                        HttpResponseMessage response = await client.PostAsync(apiUrl, content);
                        string responseData = await response.Content.ReadAsStringAsync();

                        Response.TrySkipIisCustomErrors = true;
                        if (!response.IsSuccessStatusCode)
                        {
                            Response.StatusCode = (int)response.StatusCode;
                            return Content(string.IsNullOrWhiteSpace(responseData) ? JsonConvert.SerializeObject(new { error = "Unable to delete attachment." }) : responseData, "application/json");
                        }

                        return Content(string.IsNullOrWhiteSpace(responseData) ? JsonConvert.SerializeObject(new { success = true }) : responseData, "application/json");
                    }
                }
                catch (Exception ex)
                {
                    Response.TrySkipIisCustomErrors = true;
                    Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    return Json(new
                    {
                        error = "Delete failed while forwarding document attachment.",
                        details = ex.Message
                    });
                }
            }

            private static string BuildServiceApiUrl(string baseUrl, string endpoint)
            {
                if (string.IsNullOrWhiteSpace(baseUrl))
                {
                    return endpoint;
                }

                baseUrl = baseUrl.TrimEnd('/') + "/";
                endpoint = endpoint.TrimStart('/');
                return baseUrl + endpoint;
            }
        }
    }