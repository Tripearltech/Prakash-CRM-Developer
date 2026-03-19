using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;
using PrakashCRM.Data.Models;
using System.Net.NetworkInformation;
using System.Net;
using System.Net.Sockets;
using System.Web;
using System.Configuration;
using System.Globalization;

namespace PrakashCRM.Service.Classes
{
    public class API
    {
        private static readonly SemaphoreSlim AccessTokenSemaphore;
        private static AccessToken _accessToken;
        private readonly string _clientId;
        private readonly string _clientSecret;
        private readonly string _scope;
        private readonly string _grantType;
        private readonly string _tenantId;
        private readonly string _tokenURL;
        private readonly string _environment;
        private readonly string _baseURL;
        private readonly string _companyName;
        private readonly string _codeUnitBaseUrl;
        static API()
        {
            AccessTokenSemaphore = new SemaphoreSlim(1, 1);
        }

        public API()
        {
            _clientId = System.Configuration.ConfigurationManager.AppSettings["ClientID"];
            _clientSecret = System.Configuration.ConfigurationManager.AppSettings["ClientSecret"];
            _scope = System.Configuration.ConfigurationManager.AppSettings["Scope"];
            _grantType = System.Configuration.ConfigurationManager.AppSettings["GrantType"];
            _tenantId = System.Configuration.ConfigurationManager.AppSettings["TenantID"];
            _tokenURL = System.Configuration.ConfigurationManager.AppSettings["TokenURL"];
            _environment = System.Configuration.ConfigurationManager.AppSettings["Environment"];
            _baseURL = System.Configuration.ConfigurationManager.AppSettings["BaseURL"];
            _companyName = System.Configuration.ConfigurationManager.AppSettings["CompanyName"];
            _codeUnitBaseUrl = ConfigurationManager.AppSettings["CodeUnitBaseURL"];
        }

        public async Task<AccessToken> GetAccessToken()
        {
            if (_accessToken != null && !_accessToken.Expired)
            {
                return _accessToken;
            }

            _accessToken = await FetchToken();
            return _accessToken;
        }

        private async Task<AccessToken> FetchToken()
        {
            try
            {
                await AccessTokenSemaphore.WaitAsync();

                if (_accessToken != null && !_accessToken.Expired)
                {
                    return _accessToken;
                }

                HttpClient httpClientToken = new HttpClient();

                Uri baseuri = new Uri(_tokenURL.Replace("{TenantID}", _tenantId));

                var values = new List<KeyValuePair<string, string>>();
                values.Add(new KeyValuePair<string, string>("client_id", _clientId));
                values.Add(new KeyValuePair<string, string>("client_secret", _clientSecret));
                values.Add(new KeyValuePair<string, string>("scope", _scope));
                values.Add(new KeyValuePair<string, string>("grant_type", _grantType));
                values.Add(new KeyValuePair<string, string>("resource", "https://api.businesscentral.dynamics.com/"));

                var request = new HttpRequestMessage(HttpMethod.Post, baseuri)
                {
                    Content = new FormUrlEncodedContent(values)
                };

                HttpResponseMessage response = null;
                try
                {
                    response = httpClientToken.SendAsync(request, HttpCompletionOption.ResponseHeadersRead).Result;
                }
                catch (Exception ex)
                {

                }
                AccessToken item = null;
                if (response.IsSuccessStatusCode)
                {
                    var JsonData = response.Content.ReadAsStringAsync().Result;
                    try
                    {
                        item = JsonConvert.DeserializeObject<AccessToken>(JsonData);
                    }
                    catch (Exception ex1)
                    {

                    }
                }

                return item;

            }
            catch (Exception ex)
            {
                AccessToken item = null;
                return item;
            }
            finally
            {
                AccessTokenSemaphore.Release(1);
            }
        }

        public async Task<(OData<U> items, errorDetails err)> GetData<U>(string apiendpoint, string filter)
        {
            var items = new OData<U>();
            var err = new errorDetails { isSuccess = false };

            var accessToken = await GetAccessToken().ConfigureAwait(false);
            if (accessToken == null || string.IsNullOrEmpty(accessToken.Token))
            {
                err.message = "Failed to obtain access token.";
                return (items, err);
            }

            using (var httpClient = new HttpClient())
            {
                string encodeurl = Uri.EscapeUriString(_baseURL
                    .Replace("{TenantID}", _tenantId)
                    .Replace("{Environment}", _environment)
                    .Replace("{CompanyName}", _companyName) + apiendpoint);

                var requestUri = string.IsNullOrEmpty(filter) ? encodeurl : encodeurl + "?$filter=" + Uri.EscapeDataString(filter);
                var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken.Token);

                HttpResponseMessage response;
                try
                {
                    response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    // Log ex (do not swallow)
                    err.isSuccess = false;
                    err.message = "HTTP request failed: " + ex.Message;
                    return (items, err);
                }

                err.isSuccess = response.IsSuccessStatusCode;

                string jsonData = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                if (response.IsSuccessStatusCode)
                {
                    try
                    {
                        items = JsonConvert.DeserializeObject<OData<U>>(jsonData);
                        err.code = response.StatusCode.ToString();
                        err.message = response.ReasonPhrase;
                    }
                    catch (Exception ex)
                    {
                        err.isSuccess = false;
                        err.message = "Deserialization failed: " + ex.Message;
                    }
                }
                else
                {
                    // attempt to parse structured error, otherwise return status/reason
                    try
                    {
                        if (!string.IsNullOrWhiteSpace(jsonData))
                        {
                            var emd = JObject.Parse(jsonData).ToObject<errorMaster<errorDetails>>();
                            if (emd?.error != null) err = emd.error;
                            else { err.code = response.StatusCode.ToString(); err.message = response.ReasonPhrase; }
                        }
                        else { err.code = response.StatusCode.ToString(); err.message = response.ReasonPhrase; }
                    }
                    catch (Exception ex)
                    {
                        // preserve HTTP info if error body parsing fails
                        err.code = response.StatusCode.ToString();
                        err.message = $"HTTP error and body parse failed: {ex.Message}";
                    }
                }
            }

            return (items, err);
        }

        public async Task<(OData<U> items, errorDetails err)> GetDataWithODataQuery<U>(
            string apiendpoint,
            string filter,
            int? top = null,
            int? skip = null,
            string orderBy = null)
        {
            var items = new OData<U>();
            var err = new errorDetails { isSuccess = false };

            var accessToken = await GetAccessToken().ConfigureAwait(false);
            if (accessToken == null || string.IsNullOrEmpty(accessToken.Token))
            {
                err.message = "Failed to obtain access token.";
                return (items, err);
            }

            using (var httpClient = new HttpClient())
            {
                string encodeurl = Uri.EscapeUriString(_baseURL
                    .Replace("{TenantID}", _tenantId)
                    .Replace("{Environment}", _environment)
                    .Replace("{CompanyName}", _companyName) + apiendpoint);

                var queryParts = new List<string>();

                if (!string.IsNullOrWhiteSpace(filter))
                    queryParts.Add("$filter=" + Uri.EscapeDataString(filter));

                if (top.HasValue)
                    queryParts.Add("$top=" + top.Value);

                if (skip.HasValue)
                    queryParts.Add("$skip=" + skip.Value);

                if (!string.IsNullOrWhiteSpace(orderBy))
                    queryParts.Add("$orderby=" + Uri.EscapeDataString(orderBy));

                var requestUri = queryParts.Count == 0 ? encodeurl : encodeurl + "?" + string.Join("&", queryParts);
                var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken.Token);

                HttpResponseMessage response;
                try
                {
                    response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    err.isSuccess = false;
                    err.message = "HTTP request failed: " + ex.Message;
                    return (items, err);
                }

                err.isSuccess = response.IsSuccessStatusCode;

                string jsonData = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                if (response.IsSuccessStatusCode)
                {
                    try
                    {
                        items = JsonConvert.DeserializeObject<OData<U>>(jsonData);
                        err.code = response.StatusCode.ToString();
                        err.message = response.ReasonPhrase;
                    }
                    catch (Exception ex)
                    {
                        err.isSuccess = false;
                        err.message = "Deserialization failed: " + ex.Message;
                    }
                }
                else
                {
                    try
                    {
                        if (!string.IsNullOrWhiteSpace(jsonData))
                        {
                            var emd = JObject.Parse(jsonData).ToObject<errorMaster<errorDetails>>();
                            if (emd?.error != null) err = emd.error;
                            else { err.code = response.StatusCode.ToString(); err.message = response.ReasonPhrase; }
                        }
                        else { err.code = response.StatusCode.ToString(); err.message = response.ReasonPhrase; }
                    }
                    catch (Exception ex)
                    {
                        err.code = response.StatusCode.ToString();
                        err.message = $"HTTP error and body parse failed: {ex.Message}";
                    }
                }
            }

            return (items, err);
        }

        public class ODataResponse
        {
            [JsonProperty("@odata.context")]
            public string ODataContext { get; set; }
            public string Value { get; set; } // Note: Value is a JSON string that needs further deserialization.
        }
        public async Task<(OData<U>, errorDetails)> GetDataCodeUnit<U>(string apiendpoint, object payload)
        {
            OData<U> items = new OData<U>();
            var accessToken = await GetAccessToken();

            HttpClient _httpClient = new HttpClient();
            string encodeurl = Uri.EscapeUriString(_codeUnitBaseUrl.Replace("{TenantID}", _tenantId).Replace("{Environment}", _environment).Replace("{CompanyName}", _companyName).Replace("{Endpoint}", apiendpoint));

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken.Token);

            string jsonPayload = JsonConvert.SerializeObject(payload);
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            HttpResponseMessage response = null;
            try
            {
                response = _httpClient.PostAsync(encodeurl, content).Result;
            }
            catch (Exception ex)
            {

            }

            errorDetails errordetail = new errorDetails();
            errordetail.isSuccess = response.IsSuccessStatusCode;
            if (response.IsSuccessStatusCode)
            {
                var JsonData = response.Content.ReadAsStringAsync().Result;
                try
                {
                    var odataResponse = JsonConvert.DeserializeObject<ODataResponse>(JsonData);
                    var addressList = JsonConvert.DeserializeObject<List<U>>(odataResponse.Value);

                    items.Metadata = odataResponse.ODataContext;
                    items.value = addressList;


                    errordetail.code = response.StatusCode.ToString();
                    errordetail.message = response.ReasonPhrase;
                }
                catch (Exception ex1)
                {
                }
            }
            else
            {
                var JsonData = response.Content.ReadAsStringAsync().Result;

                try
                {
                    if (JsonData != "")
                    {
                        JObject res = JObject.Parse(JsonData);
                        errorMaster<errorDetails> emd = res.ToObject<errorMaster<errorDetails>>();
                        errordetail = emd.error;
                    }
                    else
                    {
                        errordetail.code = response.StatusCode.ToString();
                        errordetail.message = response.ReasonPhrase;
                    }
                }
                catch (Exception ex1)
                {
                }
            }
            return (items, errordetail);
        }

        public async Task<(OData<U>, errorDetails)> GetData1<U>(string apiendpoint, string filter, int skip, int top, string orderby)
        {
            OData<U> items = new OData<U>();
            var accessToken = await GetAccessToken();

            HttpClient _httpClient = new HttpClient();
            string encodeurl = Uri.EscapeUriString(_baseURL.Replace("{TenantID}", _tenantId).Replace("{Environment}", _environment).Replace("{CompanyName}", _companyName) + apiendpoint);
            Uri baseuri = new Uri(encodeurl);
            var request = new HttpRequestMessage();

            if (filter == "" || filter == null)
            {
                if (skip >= 0 && top > 0)
                {
                    if (orderby != "" && orderby != null) //Mihir
                        request = new HttpRequestMessage(HttpMethod.Get, baseuri + "?$skip=" + skip + "&$top=" + top + "&$orderby=" + orderby);
                    else
                        request = new HttpRequestMessage(HttpMethod.Get, baseuri + "?$skip=" + skip + "&$top=" + top);
                }
                else
                {
                    if (orderby != "" && orderby != null) //Mihir
                        request = new HttpRequestMessage(HttpMethod.Get, baseuri + "?$orderby=" + orderby);
                    else
                        request = new HttpRequestMessage(HttpMethod.Get, baseuri);
                }
            }

            if (filter != "" && filter != null)
            {
                if (skip >= 0 && top > 0)
                {
                    if (orderby != "" && orderby != null) //Mihir
                        request = new HttpRequestMessage(HttpMethod.Get, baseuri + "?$filter=" + filter + "&$skip=" + skip + "&$top=" + top + "&$orderby=" + orderby);
                    else
                        request = new HttpRequestMessage(HttpMethod.Get, baseuri + "?$filter=" + filter + "&$skip=" + skip + "&$top=" + top);
                }
                else
                {
                    if (orderby != "" && orderby != null) //Mihir
                        request = new HttpRequestMessage(HttpMethod.Get, baseuri + "?$filter=" + filter + "&$orderby=" + orderby);
                    else
                        //request = new HttpRequestMessage(HttpMethod.Get, baseuri);
                        request = new HttpRequestMessage(HttpMethod.Get, baseuri + "?$filter=" + filter);
                }
            }

            //if (filter == "")
            //    request = new HttpRequestMessage(HttpMethod.Get, baseuri);
            //else
            //    request = new HttpRequestMessage(HttpMethod.Get, baseuri + "?$filter=" + filter);

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken.Token);

            HttpResponseMessage response = null;
            try
            {
                response = _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead).Result;
            }
            catch (Exception ex)
            {

            }

            errorDetails errordetail = new errorDetails();
            errordetail.isSuccess = response.IsSuccessStatusCode;
            if (response.IsSuccessStatusCode)
            {
                var JsonData = response.Content.ReadAsStringAsync().Result;
                try
                {
                    items = JsonConvert.DeserializeObject<OData<U>>(JsonData);

                    errordetail.code = response.StatusCode.ToString();
                    errordetail.message = response.ReasonPhrase;
                }
                catch (Exception ex1)
                {
                }
            }
            else
            {
                var JsonData = response.Content.ReadAsStringAsync().Result;

                try
                {
                    if (JsonData != "")
                    {
                        JObject res = JObject.Parse(JsonData);
                        errorMaster<errorDetails> emd = res.ToObject<errorMaster<errorDetails>>();
                        errordetail = emd.error;
                    }
                    else
                    {
                        errordetail.code = response.StatusCode.ToString();
                        errordetail.message = response.ReasonPhrase;
                    }
                }
                catch (Exception ex1)
                {
                }
            }
            return (items, errordetail);
        }

        public async Task<int> GetRecordsCount(string apiendpoint, string filter)
        {

            var accessToken = await GetAccessToken();

            HttpClient _httpClient = new HttpClient();
            string encodeurl = Uri.EscapeUriString(_baseURL.Replace("{TenantID}", _tenantId).Replace("{Environment}", _environment).Replace("{CompanyName}", _companyName) + apiendpoint);
            Uri baseuri = new Uri(encodeurl);
            //var request = new HttpRequestMessage();
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken.Token);

            //request = new HttpRequestMessage(HttpMethod.Get, baseuri + "/$count");

            //request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken.Token);

            HttpResponseMessage response = null;
            try
            {
                if (filter == "")
                    response = _httpClient.GetAsync(baseuri + "/$count").Result;
                else
                    response = _httpClient.GetAsync(baseuri + "/$count?$filter=" + filter).Result;
            }
            catch (Exception ex)
            {

            }

            if (response == null)
                return 0;

            var payload = await response.Content.ReadAsStringAsync();

            // $count should return a plain number; if we get an error payload, don't crash callers.
            if (!response.IsSuccessStatusCode || string.IsNullOrWhiteSpace(payload))
                return 0;

            payload = payload.Trim();
            if (payload.Length >= 2 && payload[0] == '"' && payload[payload.Length - 1] == '"')
                payload = payload.Substring(1, payload.Length - 2).Trim();

            if (int.TryParse(payload, NumberStyles.Integer, CultureInfo.InvariantCulture, out var countInt))
                return countInt;

            if (long.TryParse(payload, NumberStyles.Integer, CultureInfo.InvariantCulture, out var countLong))
                return (int)Math.Min(int.MaxValue, Math.Max(0, countLong));

            if (decimal.TryParse(payload, NumberStyles.Number, CultureInfo.InvariantCulture, out var countDecimal))
            {
                if (countDecimal <= 0)
                    return 0;
                if (countDecimal >= int.MaxValue)
                    return int.MaxValue;
                return (int)countDecimal;
            }

            return 0;
        }

        public async Task<int> CalculateCount(string apiendpoint, string filter)
        {
            int count = await GetRecordsCount(apiendpoint, filter);
            return count > 0 ? count : 0;
        }

        public async Task<(U, errorDetails)> PatchItem<U>(string apiendpoint, U requestModel, U responseModel, string fieldWithValue)
        {
            var accessToken = await GetAccessToken();

            HttpClient _httpClient = new HttpClient();
            string encodeurl = Uri.EscapeUriString(_baseURL.Replace("{TenantID}", _tenantId).Replace("{Environment}", _environment).Replace("{CompanyName}", _companyName) + apiendpoint);
            Uri baseuri = new Uri(encodeurl);
            var request = new HttpRequestMessage(new HttpMethod("PATCH"), baseuri + "(" + fieldWithValue + ")");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken.Token);
            _httpClient.DefaultRequestHeaders.Add("If-Match", "*");

            string ItemCardObjString = JsonConvert.SerializeObject(requestModel);
            request.Content = new StringContent(ItemCardObjString, Encoding.UTF8, "application/json");

            bool skipSiteErrorLogging = string.Equals(apiendpoint, "SiteErrorsListDotNetAPI", StringComparison.OrdinalIgnoreCase);

            HttpResponseMessage response = null;
            try
            {
                //response = _httpClient.PutAsync(baseuri, content).Result;
                response = _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead).Result;
            }
            catch (Exception ex)
            {
                if (!skipSiteErrorLogging)
                    PostSiteErrorWithResponse("PATCH_EXCEPTION", ex.Message, (baseuri + "(" + fieldWithValue + ")"), "PATCH " + apiendpoint, "TraceId: " + Guid.NewGuid().ToString("N"));
            }

            errorDetails errordetail = new errorDetails();
            if (response == null)
            {
                errordetail.isSuccess = false;
                errordetail.code = "NO_RESPONSE";
                errordetail.message = "Patch request failed: no response received.";
                return (responseModel, errordetail);
            }

            errordetail.isSuccess = response.IsSuccessStatusCode;
            if (response.IsSuccessStatusCode)
            {
                var JsonData = response.Content.ReadAsStringAsync().Result;
                try
                {
                    JObject res = JObject.Parse(JsonData);
                    responseModel = res.ToObject<U>();


                    errordetail.code = response.StatusCode.ToString();
                    errordetail.message = response.ReasonPhrase;
                }
                catch (Exception ex1)
                {
                }
            }
            else
            {
                var JsonData = response.Content.ReadAsStringAsync().Result;

                try
                {
                    JObject res = JObject.Parse(JsonData);
                    errorMaster<errorDetails> emd = res.ToObject<errorMaster<errorDetails>>();
                    errordetail = emd.error;

                }
                catch (Exception ex1)
                {
                }

                if (!skipSiteErrorLogging)
                {
                    string message = string.IsNullOrWhiteSpace(errordetail.message) ? JsonData : errordetail.message;
                    PostSiteErrorWithResponse(response.StatusCode.ToString(), message, (baseuri + "(" + fieldWithValue + ")"), "PATCH " + apiendpoint, "TraceId: " + Guid.NewGuid().ToString("N"));
                }
            }

            return (responseModel, errordetail);
        }

        public async Task<(U, errorDetails)> PostItem<U>(string apiendpoint, U requestModel, U responseModel)
        {

            var accessToken = await GetAccessToken();

            HttpClient _httpClient = new HttpClient();
            string encodeurl = Uri.EscapeUriString(_baseURL.Replace("{TenantID}", _tenantId).Replace("{Environment}", _environment).Replace("{CompanyName}", _companyName) + apiendpoint);
            Uri baseuri = new Uri(encodeurl);
            _httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + accessToken.Token);


            string ItemCardObjString = JsonConvert.SerializeObject(requestModel);
            var content = new StringContent(ItemCardObjString, Encoding.UTF8, "application/json");

            bool skipSiteErrorLogging = string.Equals(apiendpoint, "SiteErrorsListDotNetAPI", StringComparison.OrdinalIgnoreCase);

            HttpResponseMessage response = null;
            try
            {
                response = _httpClient.PostAsync(baseuri, content).Result;
            }
            catch (Exception ex)
            {
                if (!skipSiteErrorLogging)
                    PostSiteErrorWithResponse("POST_EXCEPTION", ex.Message, baseuri.ToString(), "POST " + apiendpoint, "TraceId: " + Guid.NewGuid().ToString("N"));
            }
            errorDetails errordetail = new errorDetails();
            if (response == null)
            {
                errordetail.isSuccess = false;
                errordetail.code = "NO_RESPONSE";
                errordetail.message = "Post request failed: no response received.";
                return (responseModel, errordetail);
            }

            errordetail.isSuccess = response.IsSuccessStatusCode;
            if (response.IsSuccessStatusCode)
            {
                var JsonData = response.Content.ReadAsStringAsync().Result;
                try
                {
                    JObject res = JObject.Parse(JsonData);
                    responseModel = res.ToObject<U>();

                    errordetail.code = response.StatusCode.ToString();
                    errordetail.message = response.ReasonPhrase;
                }
                catch (Exception ex1)
                {
                }
            }
            else
            {
                var JsonData = response.Content.ReadAsStringAsync().Result;

                try
                {
                    JObject res = JObject.Parse(JsonData);
                    errorMaster<errorDetails> emd = res.ToObject<errorMaster<errorDetails>>();
                    errordetail = emd.error;
                }
                catch (Exception ex1)
                {
                }

                if (!skipSiteErrorLogging)
                {
                    string message = string.IsNullOrWhiteSpace(errordetail.message) ? JsonData : errordetail.message;
                    PostSiteErrorWithResponse(response.StatusCode.ToString(), message, baseuri.ToString(), "POST " + apiendpoint, "TraceId: " + Guid.NewGuid().ToString("N"));
                }
            }
            return (responseModel, errordetail);
        }

        public async Task<(U, errorDetails)> DeleteItem<U>(string apiendpoint, U requestModel, U responseModel, string fieldWithValue)
        {
            var accessToken = await GetAccessToken();

            HttpClient _httpClient = new HttpClient();
            string encodeurl = Uri.EscapeUriString(_baseURL.Replace("{TenantID}", _tenantId).Replace("{Environment}", _environment).Replace("{CompanyName}", _companyName) + apiendpoint);
            Uri baseuri = new Uri(encodeurl);
            var request = new HttpRequestMessage(HttpMethod.Delete, baseuri + "(" + fieldWithValue + ")");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken.Token);
            _httpClient.DefaultRequestHeaders.Add("If-Match", "*");

            string ItemCardObjString = JsonConvert.SerializeObject(requestModel);
            request.Content = new StringContent(ItemCardObjString, Encoding.UTF8, "application/json");

            bool skipSiteErrorLogging = string.Equals(apiendpoint, "SiteErrorsListDotNetAPI", StringComparison.OrdinalIgnoreCase);

            HttpResponseMessage response = null;
            try
            {
                response = _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead).Result;
            }
            catch (Exception ex)
            {
                if (!skipSiteErrorLogging)
                    PostSiteErrorWithResponse("DELETE_EXCEPTION", ex.Message, (baseuri + "(" + fieldWithValue + ")"), "DELETE " + apiendpoint, "TraceId: " + Guid.NewGuid().ToString("N"));
            }

            errorDetails errordetail = new errorDetails();
            if (response == null)
            {
                errordetail.isSuccess = false;
                errordetail.code = "NO_RESPONSE";
                errordetail.message = "Delete request failed: no response received.";
                return (responseModel, errordetail);
            }

            errordetail.isSuccess = response.IsSuccessStatusCode;
            if (response.IsSuccessStatusCode)
            {
                //var JsonData = response.Content.ReadAsStringAsync().Result;
                try
                {
                    //JObject res = JObject.Parse(JsonData);
                    //responseModel = res.ToObject<U>();

                    errordetail.code = response.StatusCode.ToString();
                    errordetail.message = response.ReasonPhrase;
                }
                catch (Exception ex1)
                {

                }
            }
            else
            {
                var JsonData = response.Content.ReadAsStringAsync().Result;

                try
                {
                    JObject res = JObject.Parse(JsonData);
                    errorMaster<errorDetails> emd = res.ToObject<errorMaster<errorDetails>>();
                    errordetail = emd.error;

                }
                catch (Exception ex1)
                {
                }

                if (!skipSiteErrorLogging)
                {
                    string message = string.IsNullOrWhiteSpace(errordetail.message) ? JsonData : errordetail.message;
                    PostSiteErrorWithResponse(response.StatusCode.ToString(), message, (baseuri + "(" + fieldWithValue + ")"), "DELETE " + apiendpoint, "TraceId: " + Guid.NewGuid().ToString("N"));
                }
            }

            return (responseModel, errordetail);
        }
        public string getIPAddress()
        {
            System.Web.HttpContext context = System.Web.HttpContext.Current;
            string ipAddress = "";

            if (context != null && context.Request != null)
            {
                ipAddress = context.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
                if (string.IsNullOrWhiteSpace(ipAddress))
                    //ipAddress = context.Request.ServerVariables["REMOTE_ADDR"];

                if (string.IsNullOrWhiteSpace(ipAddress))
                    ipAddress = context.Request.UserHostAddress;
            }

            if (!string.IsNullOrWhiteSpace(ipAddress) && ipAddress.Contains(","))
                ipAddress = ipAddress.Split(',')[0].Trim();

            if (string.IsNullOrWhiteSpace(ipAddress) || ipAddress == "::1" || ipAddress == "127.0.0.1" || ipAddress == "localhost")
                ipAddress = GetLocalIPv4();

            return ipAddress;
        }

        private string GetLocalIPv4()
        {
            try
            {
                var host = Dns.GetHostEntry(Dns.GetHostName());
                foreach (var ip in host.AddressList)
                {
                    if (ip.AddressFamily == AddressFamily.InterNetwork && !IPAddress.IsLoopback(ip))
                        return ip.ToString();
                }
            }
            catch
            {
            }

            return "127.0.0.1";
        }

        public string GetMACAddress()
        {
            NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();
            string sMacAddress = string.Empty;
            foreach (NetworkInterface adapter in nics)
            {
                if (sMacAddress == string.Empty)// only return MAC Address from first card
                {
                    IPInterfaceProperties properties = adapter.GetIPProperties();
                    sMacAddress = adapter.GetPhysicalAddress().ToString();
                }
            }
            return sMacAddress;
        }

        public string getWebURL()
        {
            System.Web.HttpContext context = System.Web.HttpContext.Current;
            string WebURL = context.Request.RawUrl;

            return WebURL;
        }

        public string getBrowser()
        {
            System.Web.HttpContext context = System.Web.HttpContext.Current;
            if (context == null || context.Request == null)
                return "Unknown";

            HttpBrowserCapabilities browserbase = context.Request.Browser;
            string browser = browserbase != null ? browserbase.Browser : "";
            string version = browserbase != null ? browserbase.Version : "";

            if (string.IsNullOrWhiteSpace(browser) || browser.Equals("Unknown", StringComparison.OrdinalIgnoreCase))
            {
                browser = ParseBrowserFromUserAgent(context.Request.UserAgent);
            }

            if (!string.IsNullOrWhiteSpace(version) &&
                !browser.Contains(version) &&
                !browser.Equals("Unknown", StringComparison.OrdinalIgnoreCase))
            {
                browser += " " + version;
            }

            return browser;
        }

        private string ParseBrowserFromUserAgent(string userAgent)
        {
            if (string.IsNullOrWhiteSpace(userAgent))
                return "Unknown";

            string ua = userAgent.ToLowerInvariant();

            if (ua.Contains("edg/")) return "Microsoft Edge";
            if (ua.Contains("opr/") || ua.Contains("opera")) return "Opera";
            if (ua.Contains("chrome/") && !ua.Contains("edg/")) return "Google Chrome";
            if (ua.Contains("firefox/")) return "Mozilla Firefox";
            if (ua.Contains("safari/") && !ua.Contains("chrome/")) return "Safari";
            if (ua.Contains("msie") || ua.Contains("trident/")) return "Internet Explorer";

            return "Unknown";
        }

        public SPSiteActivity PostSiteActivity(string Code, string Description, string User)
        {
            API ac = new API();
            SPSiteActivity siteActivity = new SPSiteActivity();

            string controllerName = "";
            if (HttpContext.Current != null && HttpContext.Current.Request != null &&
                HttpContext.Current.Request.RequestContext != null &&
                HttpContext.Current.Request.RequestContext.RouteData != null &&
                HttpContext.Current.Request.RequestContext.RouteData.Values["controller"] != null)
            {
                controllerName = HttpContext.Current.Request.RequestContext.RouteData.Values["controller"].ToString();
            }

            siteActivity.Module_Name = controllerName;
            siteActivity.Trace_Id = string.IsNullOrWhiteSpace(Code) ? "ACT" : Code;
            siteActivity.IP_Address = getIPAddress();

            ////Set User browser Properties
            //HttpBrowserCapabilitiesBase browser = Request.Browser;
            siteActivity.Browser = getBrowser();

            string actionDescription = string.IsNullOrWhiteSpace(Description) ? "Accessed" : Description;
            string actionUser = string.IsNullOrWhiteSpace(User) ? "System" : User;
            siteActivity.Description = "Record " + actionDescription + " By " + actionUser;
            if (siteActivity.Description.Length > 100)
                siteActivity.Description = siteActivity.Description.Substring(0, 100);

            siteActivity.Web_URL = getWebURL();
            siteActivity.Company_Code = "";
            siteActivity.MAC_Address = GetMACAddress();
            siteActivity.Device_Name = System.Environment.MachineName;

            //HttpClient client = new HttpClient();

            //string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "Salesperson/PostSiteActivity?siteActivity =" + siteActivity;

            //client.BaseAddress = new Uri(apiUrl);
            //client.DefaultRequestHeaders.Accept.Clear();
            //client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            //string UserObjString = JsonConvert.SerializeObject(siteActivity);
            //var content = new StringContent(UserObjString, Encoding.UTF8, "application/json");

            //HttpRequestMessage request = new HttpRequestMessage
            //{
            //    Method = HttpMethod.Post,
            //    RequestUri = new Uri(apiUrl),
            //    Content = content
            //};
            //HttpResponseMessage response = await client.SendAsync(request);
            //if (response.IsSuccessStatusCode)
            //{
            //    var data1 = await response.Content.ReadAsStringAsync();
            //    siteActivity = Newtonsoft.Json.JsonConvert.DeserializeObject<SiteActivity>(data1);
            //}

            var result = ac.PostItem("SiteActivitiesListDotNetAPI", siteActivity, siteActivity);

            if (result.Result.Item1 != null)
                siteActivity = result.Result.Item1;

            return siteActivity;
        }

        public SPSiteError PostSiteError(Exception ex)
        {
            API ac = new API();

            SPSiteError siteError = new SPSiteError();
            siteError.Error_Code = ""; //ex.ErrorCode.ToString();
            siteError.Exception_Message = ex.Message.ToString();
            siteError.Exception_Stack_Trace = ex.StackTrace.ToString();
            siteError.Source = ex.Source.ToString();
            siteError.IP_Address = getIPAddress();
            siteError.Browser = getBrowser();

            siteError.Description = "Type: " + ex.GetType().ToString() + "Method Name: " + ex.TargetSite + "Current Exception: " + ex.InnerException;
            siteError.Web_URL = getWebURL();

            var result = ac.PostItem("SiteErrorsListDotNetAPI", siteError, siteError);

            if (result.Result.Item1 != null)
                siteError = result.Result.Item1;

            //HttpClient client = new HttpClient();

            //string apiUrl = ConfigurationManager.AppSettings["ServiceApiUrl"].ToString() + "Salesperson/PostSiteError?siteError =" + siteError;

            //client.BaseAddress = new Uri(apiUrl);
            //client.DefaultRequestHeaders.Accept.Clear();
            //client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            //string UserObjString = JsonConvert.SerializeObject(siteError);
            //var content = new StringContent(UserObjString, Encoding.UTF8, "application/json");

            //HttpRequestMessage request = new HttpRequestMessage
            //{
            //    Method = HttpMethod.Post,
            //    RequestUri = new Uri(apiUrl),
            //    Content = content
            //};
            //HttpResponseMessage response = await client.SendAsync(request);

            //if (response.IsSuccessStatusCode)
            //{
            //    var data1 = await response.Content.ReadAsStringAsync();
            //    siteError = Newtonsoft.Json.JsonConvert.DeserializeObject<SiteError>(data1);
            //}

            return siteError;
        }

        public SPSiteError PostSiteErrorWithResponse(string Code, string Message, string RequestURL, string Method, string Description = "")
        {
            API ac = new API();

            SPSiteError siteError = new SPSiteError();
            siteError.Error_Code = Code;
            siteError.Exception_Message = Message;
            siteError.Exception_Stack_Trace = RequestURL;
            siteError.Source = Method;
            siteError.IP_Address = getIPAddress();
            siteError.Browser = getBrowser();

            siteError.Description = string.IsNullOrWhiteSpace(Description) ? ("TraceId: " + Guid.NewGuid().ToString("N")) : Description;
            siteError.Web_URL = getWebURL();

            var result = ac.PostItem("SiteErrorsListDotNetAPI", siteError, siteError);

            if (result.Result.Item1 != null)
                siteError = result.Result.Item1;

            return siteError;
        }
    }
}