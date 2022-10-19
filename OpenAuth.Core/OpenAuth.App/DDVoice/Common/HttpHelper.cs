using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace OpenAuth.App.DDVoice.Common
{
    public class ResultResponse
    {
        public ResultResponse()
        {
            IsSuccess = true;
            Result = "";
            ErrorMsg = "";
        }
        /// <summary>
        /// 是否成功
        /// </summary>
        public bool IsSuccess { get; set; }
        /// <summary>
        /// 返回结果(Josn)
        /// </summary>
        public string Result { get; set; }
        /// <summary>
        /// 异常信息
        /// </summary>
        public string ErrorMsg { get; set; }
    }

    /// <summary>
    /// 返回实体
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ResultResponse<T>
    {
        public ResultResponse()
        {
            this.Success = true;
            this.Message = "成功";
        }
        /// <summary>
        /// 返回结果
        /// </summary>
        public T Result { get; set; }
        /// <summary>
        /// 是否成功
        /// </summary>
        public bool Success { get; set; }
        /// <summary>
        /// 信息
        /// </summary>
        public string Message { get; set; }
    }

    /// <summary>
    /// 请求
    /// </summary>
    public class HttpHelpers
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="url"></param>
        /// <param name="postData"></param>
        /// <param name="contentType"></param>
        /// <param name="timeOut"></param>
        /// <param name="headers"></param>
        /// <returns></returns>
        public static ResultResponse HttpPostAsync(string url, string postData, string contentType = null, int timeOut = 30, Dictionary<string, string> headers = null)
        {
            ResultResponse responseResult = new ResultResponse() { ErrorMsg = "", Result = "", IsSuccess = true };
            Uri uri = new Uri(url);
            HttpWebRequest httpWebRequest = WebRequest.Create(uri) as HttpWebRequest;
            if (headers != null)
            {
                foreach (var header in headers)
                {
                    httpWebRequest.Headers[header.Key] = header.Value;
                }
            }
            byte[] bytesToPost = Encoding.UTF8.GetBytes(postData);//转换请求数据为二进制
            httpWebRequest.Method = "POST";
            httpWebRequest.KeepAlive = false;
            httpWebRequest.AllowAutoRedirect = true;
            httpWebRequest.ContentType = "application/json; charset=UTF-8";
            httpWebRequest.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64; rv:5.0.1) Gecko/20100101 Firefox/5.0.1";
            httpWebRequest.ContentLength = bytesToPost.Length;
            System.IO.Stream outputStream = httpWebRequest.GetRequestStream();
            outputStream.Write(bytesToPost, 0, bytesToPost.Length);
            outputStream.Close();
            try
            {
                HttpWebResponse response = httpWebRequest.GetResponse() as HttpWebResponse;
                using (StreamReader sr = new StreamReader(response.GetResponseStream()))
                {
                    responseResult.Result = sr.ReadToEnd();
                }
            }
            catch (System.Net.WebException webException)
            {
                string errorMsg = webException.Message;
                responseResult.IsSuccess = false;
                responseResult.ErrorMsg = errorMsg;
            }
            return responseResult;
        }

        /// <summary>
        /// 异步请求
        /// </summary>
        /// <param name="url">url</param>
        /// <param name="strAccessToken">token</param>
        /// <param name="postDataStr">参数</param>
        /// <param name="method">请求方式</param>
        /// <returns></returns>
        public static async Task<ResultResponse> HttpPostAsync(string url, string strAccessToken, string postDataStr, string method = "post")
        {
            ResultResponse responseResult = new ResultResponse() { ErrorMsg = "", Result = "", IsSuccess = true };
            StringBuilder queryBuilder = new StringBuilder();
            if ("GET".Equals(method.ToUpper()))
            {
                string uriStr = url;
                if (!string.IsNullOrEmpty(postDataStr))
                {
                    uriStr = uriStr + "?" + postDataStr;
                }
                Uri uri = new Uri(uriStr);
                HttpWebRequest httpWebRequest = WebRequest.Create(uri) as HttpWebRequest;
                httpWebRequest.Method = "GET";
                httpWebRequest.KeepAlive = false;
                httpWebRequest.AllowAutoRedirect = true;
                httpWebRequest.ContentType = "application/json; charset=UTF-8";
                httpWebRequest.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64; rv:5.0.1) Gecko/20100101 Firefox/5.0.1";
                HttpWebResponse response = await httpWebRequest.GetResponseAsync() as HttpWebResponse;
                using (StreamReader sr = new StreamReader(response.GetResponseStream()))
                {
                    responseResult.Result = sr.ReadToEnd();
                }
            }
            else
            {
                byte[] bytesToPost = Encoding.UTF8.GetBytes(postDataStr);//转换请求数据为二进制
                Uri uri = new Uri(url);
                HttpWebRequest httpWebRequest = WebRequest.Create(uri) as HttpWebRequest;
                httpWebRequest.Method = "POST";
                httpWebRequest.KeepAlive = false;
                httpWebRequest.AllowAutoRedirect = true;
                httpWebRequest.ContentType = "application/json; charset=UTF-8";
                //  httpWebRequest.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64; rv:5.0.1) Gecko/20100101 Firefox/5.0.1";
                httpWebRequest.ContentLength = bytesToPost.Length;
                System.IO.Stream outputStream = httpWebRequest.GetRequestStream();
                outputStream.Write(bytesToPost, 0, bytesToPost.Length);
                outputStream.Close();
                try
                {
                    HttpWebResponse response = await httpWebRequest.GetResponseAsync() as HttpWebResponse;
                    using (StreamReader sr = new StreamReader(response.GetResponseStream()))
                    {
                        responseResult.Result = sr.ReadToEnd();
                    }
                }
                catch (System.Net.WebException webException)
                {
                    string errorMsg = webException.Message;
                    responseResult.IsSuccess = false;
                    responseResult.ErrorMsg = errorMsg;
                }
            }
            return responseResult;
        }

        /// <summary>
        /// 同步请求post
        /// </summary>
        /// <param name="url"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static ResultResponse HttpPost(string url, string data)
        {
            byte[] bytesToPost = System.Text.Encoding.Default.GetBytes(data); //转换为bytes数据
            ResultResponse responseResult = new ResultResponse() { ErrorMsg = "", Result = "", IsSuccess = true };
            HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(url);
            try
            {
                req.Method = "POST";
                req.ContentType = "application/json; charset=UTF-8";//application/x-www-form-urlencoded;
                req.ContentLength = bytesToPost.Length;
                //   req.Timeout = 60 * 5 * 1000;//5分钟
                using (Stream reqStream = req.GetRequestStream())
                {
                    reqStream.Write(bytesToPost, 0, bytesToPost.Length);     //把要上传网页系统的数据通过post发送
                }
                HttpWebResponse cnblogsRespone = (HttpWebResponse)req.GetResponse();
                if (cnblogsRespone != null && cnblogsRespone.StatusCode == HttpStatusCode.OK)
                {
                    using (StreamReader sr = new StreamReader(cnblogsRespone.GetResponseStream()))
                    {
                        responseResult.Result = sr.ReadToEnd();
                    }
                }
            }
            catch (Exception webException)
            {
                string errorMsg = webException.Message;
                responseResult.IsSuccess = false;
                responseResult.ErrorMsg = errorMsg;
            }
            return responseResult;
        }

        /// <summary>
        /// From请求Post
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <param name="postDataStr">Body请求数据</param>
        /// <returns></returns>
        public static ResultResponse HttpPostFrom(string url, string postDataStr)
        {
            ResultResponse responseResult = new ResultResponse() { ErrorMsg = "", Result = "", IsSuccess = true };
            HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(url);
            try
            {
                byte[] bytesToPost = Encoding.UTF8.GetBytes(postDataStr);
                req.Method = "POST";
                req.ContentType = "application/x-www-form-urlencoded;charset=UTF-8";
                req.ContentLength = bytesToPost.Length;
                using (Stream reqStream = req.GetRequestStream())
                {
                    reqStream.Write(bytesToPost, 0, bytesToPost.Length);
                }
                HttpWebResponse response = (HttpWebResponse)req.GetResponse();
                if (response != null && response.StatusCode == HttpStatusCode.OK)
                {
                    using (StreamReader sr = new StreamReader(response.GetResponseStream(), Encoding.GetEncoding("utf-8")))
                    {
                        responseResult.Result = sr.ReadToEnd();
                    }
                }
            }
            catch (Exception webException)
            {
                string errorMsg = webException.Message;
                responseResult.IsSuccess = false;
                responseResult.ErrorMsg = errorMsg;
            }
            return responseResult;
        }

        /// <summary>
        /// 同步请求post
        /// </summary>
        /// <param name="url"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static ResultResponse HttpPostForm(string url, string data)
        {
            byte[] bytesToPost = System.Text.Encoding.Default.GetBytes(data); //转换为bytes数据
            ResultResponse responseResult = new ResultResponse() { ErrorMsg = "", Result = "", IsSuccess = true };
            HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(url);
            try
            {
                req.Method = "POST";
                req.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
                req.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/85.0.4183.83 Safari/537.36";
                req.ContentLength = bytesToPost.Length;
                //   req.Timeout = 60 * 5 * 1000;//5分钟
                using (Stream reqStream = req.GetRequestStream())
                {
                    reqStream.Write(bytesToPost, 0, bytesToPost.Length);
                }
                HttpWebResponse cnblogsRespone = (HttpWebResponse)req.GetResponse();
                if (cnblogsRespone != null && cnblogsRespone.StatusCode == HttpStatusCode.OK)
                {
                    using (StreamReader sr = new StreamReader(cnblogsRespone.GetResponseStream()))
                    {
                        responseResult.Result = sr.ReadToEnd();
                    }
                }
            }
            catch (Exception webException)
            {
                string errorMsg = webException.Message;
                responseResult.IsSuccess = false;
                responseResult.ErrorMsg = errorMsg;
            }
            return responseResult;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="url"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static ResultResponse HttpGet(string url, string data)
        {
            ResultResponse responseResult = new ResultResponse() { ErrorMsg = "", Result = "", IsSuccess = true };
            try
            {
                string uriStr = url;
                if (!string.IsNullOrEmpty(data))
                {
                    uriStr = uriStr + "?" + data;
                }
                Uri uri = new Uri(uriStr);
                HttpWebRequest httpWebRequest = WebRequest.Create(uri) as HttpWebRequest;
                httpWebRequest.Method = "GET";
                httpWebRequest.KeepAlive = false;
                httpWebRequest.AllowAutoRedirect = true;
                httpWebRequest.Timeout = 3 * 60 * 5 * 1000;//15分钟
                httpWebRequest.ContentType = "application/json; charset=UTF-8";
                httpWebRequest.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64; rv:5.0.1) Gecko/20100101 Firefox/5.0.1";
                HttpWebResponse response = httpWebRequest.GetResponse() as HttpWebResponse;
                using (StreamReader sr = new StreamReader(response.GetResponseStream()))
                {
                    responseResult.IsSuccess = true;
                    responseResult.Result = sr.ReadToEnd();
                }
            }
            catch (Exception ex)
            {
                responseResult.IsSuccess = false;
                responseResult.ErrorMsg = ex.Message;
                responseResult.Result = "error";
            }
            return responseResult;
        }

        /// <summary>
        /// 异步请求post
        /// </summary>
        /// <param name="url"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public async static Task<ResultResponse> HttpPostAsync(string url, object data, AuthenticationHeaderValue authenticationHeader)
        {
            string jsonData = Newtonsoft.Json.JsonConvert.SerializeObject(data);
            ResultResponse responseResult = new ResultResponse();
            HttpClient httpClient = new HttpClient();
            if (authenticationHeader != null)
            {
                httpClient.Timeout = TimeSpan.FromSeconds(60 * 5);
                httpClient.DefaultRequestHeaders.Authorization = authenticationHeader;
            }
            HttpContent contentPost = new StringContent(jsonData, Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync(url, contentPost);
            if (!response.IsSuccessStatusCode)
            {
                responseResult.ErrorMsg = response.StatusCode.ToString();
                responseResult.IsSuccess = false;
            }
            else
            {
                responseResult.Result = await response.Content.ReadAsStringAsync();
            }
            return responseResult;
        }

        /// <summary>
        /// 请求post
        /// </summary>
        /// <param name="jsonData">json输入字符</param>
        /// <param name="uri"></param>
        /// <returns>返回的WebResponse输出</returns>
        public static ResultResponse PostAuthorization(string uri, string jsonData, string token)
        {
            ResultResponse responseResult = new ResultResponse();
            Uri address = new Uri(uri);
            HttpWebRequest request;
            string returnXML = string.Empty;
            if (address == null) { throw new ArgumentNullException("address"); }
            try
            {
                request = WebRequest.Create(address) as HttpWebRequest;
                request.Method = "POST";
                request.ContentType = "application/json";
                request.Headers.Add("Authorization", "Basic " + token);
                if (jsonData != null)
                {
                    byte[] byteData = UTF8Encoding.UTF8.GetBytes(jsonData);
                    request.ContentLength = byteData.Length;
                    using (Stream postStream = request.GetRequestStream())
                    {
                        postStream.Write(byteData, 0, byteData.Length);
                    }
                    using (HttpWebResponse response1 = request.GetResponse() as HttpWebResponse)
                    {
                        using (StreamReader reader = new StreamReader(response1.GetResponseStream(), Encoding.UTF8))
                        {
                            responseResult.Result = reader.ReadToEnd();
                            responseResult.IsSuccess = true;
                        }
                    }
                }
            }
            catch (WebException wex)
            {
                if (wex.Response != null)
                {
                    using (HttpWebResponse errorResponse = (HttpWebResponse)wex.Response)
                    {
                        try
                        {
                            string sError = string.Format("The server returned '{0}' with the status code {1} ({2:d}).",
                            errorResponse.StatusDescription, errorResponse.StatusCode,
                            errorResponse.StatusCode);
                            using (StreamReader sr = new StreamReader(errorResponse.GetResponseStream(), Encoding.UTF8))
                            {
                                responseResult.ErrorMsg = sr.ReadToEnd();
                                responseResult.IsSuccess = false;
                            }
                        }
                        finally
                        {
                            if (errorResponse != null) errorResponse.Close();
                        }
                    }
                }
            }
            return responseResult;
        }

        /// <summary>
        /// 请求Post
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="keyValuePairs"></param>
        /// <returns></returns>
        public static string WebClientPost(string uri, Dictionary<string, string> keyValuePairs)
        {
            WebClient WebClientObj = new WebClient();
            System.Collections.Specialized.NameValueCollection PostVars = new System.Collections.Specialized.NameValueCollection();
            foreach (var item in keyValuePairs)
            {
                PostVars.Add(item.Key, item.Value);
            }
            byte[] byRemoteInfo = WebClientObj.UploadValues(uri, "POST", PostVars);
            return Encoding.Default.GetString(byRemoteInfo);
        }

        /// <summary>
        /// 同步请求post
        /// </summary>
        /// <param name="url"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static ResultResponse HttpPost(string url, object data)
        {
            string json = JsonConvert.SerializeObject(data); //转换为bytes数据
            ResultResponse responseResult = new ResultResponse() { ErrorMsg = "", Result = "", IsSuccess = true };
            HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(url);
            try
            {
                req.Method = "POST";
                req.Headers.Add("accept: text/plain");
                req.Headers.Add("Accept-Encoding: gzip, deflate");
                req.Headers.Add("Accept-Language: en-US,en;q=0.9,zh-CN;q=0.8,zh;q=0.7");
                req.Headers.Add("Connection: keep-alive");
                req.Headers.Add("Content-Length: 160");
                req.Headers.Add("Content-Type: application/json-patch+json;charset=utf-8");
                req.Headers.Add("User-Agent: Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/76.0.3809.132 Safari/537.36");
                req.ContentLength = Encoding.UTF8.GetByteCount(json);
                using (var streamWriter = new StreamWriter(req.GetRequestStream()))
                {
                    streamWriter.Write(json);     //把要上传网页系统的数据通过post发送
                }
                HttpWebResponse cnblogsRespone = (HttpWebResponse)req.GetResponse();
                if (cnblogsRespone != null && cnblogsRespone.StatusCode == HttpStatusCode.OK)
                {
                    using (StreamReader sr = new StreamReader(cnblogsRespone.GetResponseStream()))
                    {
                        responseResult.Result = sr.ReadToEnd();
                    }
                }
            }
            catch (Exception webException)
            {
                string errorMsg = webException.Message;
                responseResult.IsSuccess = false;
                responseResult.ErrorMsg = errorMsg;
            }
            return responseResult;
        }

        /// <summary>
        /// 同步请求post
        /// </summary>
        /// <param name="url"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static ResultResponse HttpPost(string url, object data, string contentType)
        {
            string json = JsonConvert.SerializeObject(data); //转换为bytes数据
            ResultResponse responseResult = new ResultResponse() { ErrorMsg = "", Result = "", IsSuccess = true };
            HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(url);
            try
            {
                req.Method = "POST";
                req.Headers.Add("accept: Accept:text/plain,text/html");
                req.Headers.Add("Accept-Encoding:gzip,deflate");
                req.Headers.Add("Connection: keep-alive");
                req.Headers.Add("Content-Type:" + contentType);
                req.Headers.Add("Accept-Language:en,zh");
                req.Headers.Add("User-Agent: Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/76.0.3809.132 Safari/537.36");
                req.ContentLength = Encoding.UTF8.GetByteCount(json);
                using (var streamWriter = new StreamWriter(req.GetRequestStream()))
                {
                    streamWriter.Write(json);     //把要上传网页系统的数据通过post发送
                }
                HttpWebResponse cnblogsRespone = (HttpWebResponse)req.GetResponse();
                if (cnblogsRespone != null && cnblogsRespone.StatusCode == HttpStatusCode.OK)
                {
                    using (StreamReader sr = new StreamReader(cnblogsRespone.GetResponseStream(), Encoding.GetEncoding("utf-8")))
                    {
                        responseResult.Result = sr.ReadToEnd();
                    }
                }
            }
            catch (Exception webException)
            {
                string errorMsg = webException.Message;
                responseResult.IsSuccess = false;
                responseResult.ErrorMsg = errorMsg;
            }
            return responseResult;
        }

        /// <summary>
        /// Get异步请求
        /// </summary>
        /// <param name="url">请求Url</param>
        /// <param name="strAccessToken">token</param>
        /// <returns></returns>
        public static async Task<ResultResponse> HttpGetAsync(string url, string strAccessToken)
        {
            ResultResponse responseResult = new ResultResponse();
            HttpClient httpClient = new HttpClient();
            HttpContent contentPost = new StringContent(url, Encoding.UTF8, "application/json");
            var response = await httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                responseResult.ErrorMsg = response.StatusCode.ToString();
                responseResult.IsSuccess = false;
            }
            else
            {
                responseResult.Result = await response.Content.ReadAsStringAsync();
            }
            return responseResult;
        }

        /// <summary>
        /// Get异步请求(Token)
        /// </summary>
        /// <param name="url">请求Url</param>
        /// <param name="strAccessToken">token</param>
        /// <returns></returns>
        public static ResultResponse HttpGetTokenAsync(string url, string strAccessToken)
        {
            ResultResponse responseResult = new ResultResponse();
            HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(url);
            Encoding encoding = Encoding.UTF8;
            var headers = req.Headers;
            headers["Authorization"] = strAccessToken; //传递进去认证Token
            req.Headers = headers;
            string responseData = String.Empty;
            req.Method = "GET";
            using (HttpWebResponse response = (HttpWebResponse)req.GetResponse())
            {
                using (StreamReader reader = new StreamReader(response.GetResponseStream(), encoding))
                {
                    responseData = reader.ReadToEnd();
                }
                responseResult.Result = JsonConvert.DeserializeObject<ResponResult>(responseData).data;
                return responseResult;
            }
        }

        /// <summary>
        /// Get异步请求
        /// </summary>
        /// <param name="url"></param>
        /// <param name="strAccessToken"></param>
        /// <param name="authenticationHeader"></param>
        /// <returns></returns>
        public static async Task<ResultResponse> HttpGetAsync(string url, string strAccessToken, AuthenticationHeaderValue authenticationHeader)
        {
            ResultResponse responseResult = new ResultResponse();
            HttpClient httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = authenticationHeader;
            HttpContent contentPost = new StringContent(url, Encoding.UTF8, "application/json");
            var response = await httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                responseResult.ErrorMsg = response.StatusCode.ToString();
                responseResult.IsSuccess = false;
            }
            else
            {
                responseResult.Result = await response.Content.ReadAsStringAsync();
            }
            return responseResult;
        }

        /// <summary>
        /// Get异步请求
        /// </summary>
        /// <param name="url">请求Url</param>
        /// <param name="strAccessToken">token</param>
        /// <returns></returns>
        public static async Task<string> HttpGetAsync(string url, Dictionary<string, string> headers = null)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                if (headers != null)
                {
                    foreach (var header in headers)
                        request.Headers[header.Key] = header.Value;
                }
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    Stream responseStream = response.GetResponseStream();
                    StreamReader streamReader = new StreamReader(responseStream, Encoding.UTF8);
                    return await streamReader.ReadToEndAsync();
                }
            }
            catch (Exception ex)
            {
                return await Task.FromResult(ex.Message);
            }
        }

        /// <summary>
        /// Post请求
        /// </summary>
        /// <param name="url"></param>
        /// <param name="postData"></param>
        /// <param name="contentType"></param>
        /// <param name="timeOut"></param>
        /// <param name="headers"></param>
        /// <returns></returns>
        public static string HttpPost(string url, string postData, Dictionary<string, string> headers = null)
        {
            Uri uri = new Uri(url);
            HttpWebRequest httpWebRequest = WebRequest.Create(uri) as HttpWebRequest;
            if (headers != null)
            {
                foreach (var header in headers)
                {
                    httpWebRequest.Headers[header.Key] = header.Value;
                }
            }
            byte[] bytesToPost = Encoding.UTF8.GetBytes(postData);//转换请求数据为二进制
            httpWebRequest.Method = "POST";
            httpWebRequest.KeepAlive = false;
            httpWebRequest.AllowAutoRedirect = true;
            httpWebRequest.ContentType = "application/json; charset=UTF-8";
            httpWebRequest.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64; rv:5.0.1) Gecko/20100101 Firefox/5.0.1";
            httpWebRequest.ContentLength = bytesToPost.Length;
            using (Stream outStream = httpWebRequest.GetRequestStream())
            {
                outStream.Write(bytesToPost, 0, bytesToPost.Length);
            }
            string result = "";
            try
            {
                HttpWebResponse response = httpWebRequest.GetResponse() as HttpWebResponse;
                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                {
                    result = reader.ReadToEnd().Trim();
                }
            }
            catch (System.Net.WebException webException)
            {
                //HttpWebResponse response = webException.Response as HttpWebResponse;
                //Stream responseStream = response.GetResponseStream();
                string errorMsg = webException.Message;
            }
            return result;
        }

        /// <summary>
        /// Get异步请求
        /// </summary>
        /// <param name="url">请求Url</param>
        /// <param name="strAccessToken">token</param>
        /// <returns></returns>
        public static async Task<ResultResponse> HttpGetAsync(string url, SortedDictionary<string, object> sortedDictionary, string strAccessToken)
        {
            ResultResponse responseResult = new ResultResponse();
            HttpClient httpClient = new HttpClient();
            if (sortedDictionary != null && sortedDictionary.Count > 0)
            {
                foreach (var item in sortedDictionary)
                {
                    httpClient.DefaultRequestHeaders.Add(item.Key, item.Value.ToString());
                }
            }
            url += "?" + string.Join("&", sortedDictionary.Select(t => t.Key + "=" + t.Value));
            HttpContent contentPost = new StringContent(url, Encoding.UTF8, "application/json");
            var response = await httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                responseResult.ErrorMsg = response.StatusCode.ToString();
                responseResult.IsSuccess = false;
            }
            else
            {
                responseResult.Result = await response.Content.ReadAsStringAsync();
            }
            return responseResult;
        }

        /// <summary>
        /// 异步Post请求
        /// </summary>
        /// <param name="url">url</param>
        /// <param name="data">数据</param>
        /// <param name="cert">证书</param>
        /// <returns></returns>
        public static async Task<string> PostAsync(string url, string data, string token = null, X509Certificate2 cert = null)
        {
            return await Task.Run(() => Post(url, data, token, cert));
        }

        /// <summary>
        /// Post请求
        /// </summary>
        /// <param name="url">url</param>
        /// <param name="data">数据</param>
        /// <param name="cert">证书</param>
        /// <returns></returns>
        public static string Post(string url, string data, string token = null, X509Certificate2 cert = null)
        {
            try
            {
                if (url.StartsWith("https", StringComparison.OrdinalIgnoreCase))
                {
                    ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(CheckValidationResult);
                }
                byte[] dataByte = Encoding.UTF8.GetBytes(data);
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                if (!string.IsNullOrEmpty(token))
                {
                    request.Headers.Add("Authorization", "Bearer " + token);
                }
                request.Method = "POST";
                request.ContentType = "application/json;charset=utf-8";
                request.ContentLength = dataByte.Length;
                if (cert != null)
                {
                    request.ClientCertificates.Add(cert);
                }
                using (Stream outStream = request.GetRequestStream())
                {
                    outStream.Write(dataByte, 0, dataByte.Length);
                }
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                        {
                            return reader.ReadToEnd().Trim();
                        }
                    }
                    else
                    {
                        var result = new
                        {
                            success = false,
                            errorMessage = "",
                            result = "",
                            resultCode = response.StatusCode.ToString(),
                        };
                        return JsonConvert.SerializeObject(result);
                    }
                }
            }
            catch (WebException webException)
            {
                if (HttpStatusCode.Unauthorized == ((HttpWebResponse)webException.Response).StatusCode)
                {
                    var result = new
                    {
                        success = false,
                        errorMessage = webException.Message,
                        result = "",
                        resultCode = "401",
                    };
                    return JsonConvert.SerializeObject(result);
                }
                return webException.Message;
            }
        }

        public static string Post(string url, string data, ref WebHeaderCollection responseHeaders)
        {
            try
            {
                byte[] dataByte = Encoding.UTF8.GetBytes(data);
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "POST";
                request.ContentType = "application/json;charset=utf-8";
                request.ContentLength = dataByte.Length;
                using (Stream outStream = request.GetRequestStream())
                {
                    outStream.Write(dataByte, 0, dataByte.Length);
                }
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        responseHeaders = response.Headers;
                        using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                        {
                            return reader.ReadToEnd().Trim();
                        }
                    }
                    else if (response.StatusCode == HttpStatusCode.Created)
                    {
                        responseHeaders = response.Headers;
                        using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                        {
                            return reader.ReadToEnd().Trim();
                        }
                    }
                }
                return "";
            }
            catch (WebException webException)
            {
                if (HttpStatusCode.Unauthorized == ((HttpWebResponse)webException.Response).StatusCode)
                {
                    var result = new
                    {
                        success = false,
                        errorMessage = webException.Message,
                        result = "",
                        resultCode = "401",
                    };
                    return JsonConvert.SerializeObject(result);
                }
                return webException.Message;
            }
        }

        private static bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
        {
            return true;
        }

        /// <summary>
        /// 异步Post请求
        /// </summary>
        /// <param name="url">url</param>
        /// <returns></returns>
        public static async Task<string> GetAsync(string url, string data, string token = null, X509Certificate2 cert = null)
        {
            return await Task.Run(() => Get(url, data, token, cert));
        }
        /// <summary>
        /// Get请求
        /// </summary>
        /// <param name="url">url</param>
        /// <returns></returns>
        public static string Get(string url, string data, string token = null, X509Certificate2 cert = null)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            if (url.StartsWith("https", StringComparison.OrdinalIgnoreCase))
            {
                ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(CheckValidationResult);
            }
            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Add("Authorization", "Bearer " + token);
            }
            if (!string.IsNullOrEmpty(data))
            {
                byte[] dataByte = Encoding.UTF8.GetBytes(data);
                request.ContentLength = dataByte.Length;
            }
            request.ContentType = "application/json;charset=utf-8";
            if (cert != null)
            {
                request.ClientCertificates.Add(cert);
            }
            using (WebResponse response = request.GetResponse())
            {
                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                {
                    return reader.ReadToEnd().Trim();
                }
            }
        }
        /// <summary>
        /// Get请求
        /// </summary>
        /// <param name="url">url</param>
        /// <returns></returns>
        public static string Get(string url)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "GET";
            request.ContentType = "application/x-www-form-urlencoded;charset=utf-8";
            using (WebResponse response = request.GetResponse())
            {
                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                {
                    return reader.ReadToEnd().Trim();
                }
            }
        }
        ///// <summary>
        ///// Post异步请求
        ///// </summary>
        ///// <param name="url">请求Url</param>
        ///// <param name="requestData">请求数据</param>
        ///// <param name="strAccessToken">token</param>
        ///// <returns></returns>
        //public async Task<ResultResponse> HttpPostAsync(string url, string requestData, string strAccessToken)
        //{
        //    try
        //    {
        //        ResultResponse responseResult = new ResultResponse();
        //        HttpClient httpClient = new HttpClient();
        //        httpClient.SetBearerToken(strAccessToken);
        //        HttpContent contentPost = new StringContent(requestData, Encoding.UTF8, "application/json");
        //        var response = await httpClient.PostAsync(url, contentPost);
        //        if (!response.IsSuccessStatusCode)
        //        {
        //            responseResult.ErrorMsg = response.StatusCode.ToString();
        //            responseResult.IsSuccess = false;
        //        }
        //        else
        //        {
        //            responseResult.Result = await response.Content.ReadAsStringAsync();
        //        }
        //        return responseResult;
        //    }
        //    catch (Exception ex)
        //    {
        //        string msg = ex.Message;
        //        throw;
        //    }

        //}

        ///// <summary>
        ///// Post异步请求（帶消息頭）
        ///// </summary>
        ///// <param name="url">请求Url</param>
        ///// <param name="requestData">请求数据</param>
        ///// <param name="strAccessToken">token</param>
        ///// <returns></returns>
        //public async Task<ResponseResult> RequesResultAsyncPost(string url, string requestData, string strAccessToken, SortedDictionary<string, object> headDictionary = null)
        //{
        //    try
        //    {
        //        ResponseResult responseResult = new ResponseResult();
        //        HttpClient httpClient = new HttpClient();
        //        httpClient.SetBearerToken(strAccessToken);
        //        //設置消息頭
        //        if (headDictionary != null && headDictionary.Count > 0)
        //        {
        //            foreach (var item in headDictionary)
        //            {
        //                httpClient.DefaultRequestHeaders.Add(item.Key, item.Value.ToString());
        //            }
        //        }
        //        HttpContent contentPost = new StringContent(requestData, Encoding.UTF8, "application/json");
        //        var response = await httpClient.PostAsync(url, contentPost);
        //        if (!response.IsSuccessStatusCode)
        //        {
        //            responseResult.ErrorMsg = response.StatusCode.ToString();
        //            responseResult.IsSuccess = false;
        //        }
        //        else
        //        {
        //            responseResult.Result = await response.Content.ReadAsStringAsync();
        //        }
        //        return responseResult;
        //    }
        //    catch (Exception ex)
        //    {
        //        string msg = ex.Message;
        //        throw;
        //    }

        //}

    }
    /// <summary>
    /// 请求
    /// </summary>
    public class HttpHelper<T>
    {
        /// <summary>
        /// Get异步请求
        /// </summary>
        /// <param name="url">请求Url</param>
        /// <param name="strAccessToken">token</param>
        /// <returns></returns>
        public static async Task<ResultResponse<T>> HttpGetAsync(string url, SortedDictionary<string, object> sortedDictionary, string strAccessToken)
        {
            ResultResponse<T> responseResult = new ResultResponse<T>();
            HttpClient httpClient = new HttpClient();
            if (sortedDictionary != null && sortedDictionary.Count > 0)
            {
                foreach (var item in sortedDictionary)
                {
                    httpClient.DefaultRequestHeaders.Add(item.Key, item.Value.ToString());
                }
            }
            url += "?" + string.Join("&", sortedDictionary.Select(t => t.Key + "=" + t.Value));
            HttpContent contentPost = new StringContent(url, Encoding.UTF8, "application/json");
            var response = await httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                responseResult.Message = response.StatusCode.ToString();
                responseResult.Success = false;
            }
            else
            {
                string result = await response.Content.ReadAsStringAsync();
                responseResult.Result = JsonConvert.DeserializeObject<T>(result);
            }
            return responseResult;
        }
        ///// <summary>
        ///// Post异步请求
        ///// </summary>
        ///// <param name="url">请求Url</param>
        ///// <param name="requestData">请求数据</param>
        ///// <param name="strAccessToken">token</param>
        ///// <returns></returns>
        //public async Task<ResultResponse> HttpPostAsync(string url, string requestData, string strAccessToken)
        //{
        //    try
        //    {
        //        ResultResponse responseResult = new ResultResponse();
        //        HttpClient httpClient = new HttpClient();
        //        httpClient.SetBearerToken(strAccessToken);
        //        HttpContent contentPost = new StringContent(requestData, Encoding.UTF8, "application/json");
        //        var response = await httpClient.PostAsync(url, contentPost);
        //        if (!response.IsSuccessStatusCode)
        //        {
        //            responseResult.ErrorMsg = response.StatusCode.ToString();
        //            responseResult.IsSuccess = false;
        //        }
        //        else
        //        {
        //            responseResult.Result = await response.Content.ReadAsStringAsync();
        //        }
        //        return responseResult;
        //    }
        //    catch (Exception ex)
        //    {
        //        string msg = ex.Message;
        //        throw;
        //    }

        //}

        ///// <summary>
        ///// Post异步请求（帶消息頭）
        ///// </summary>
        ///// <param name="url">请求Url</param>
        ///// <param name="requestData">请求数据</param>
        ///// <param name="strAccessToken">token</param>
        ///// <returns></returns>
        //public async Task<ResponseResult> RequesResultAsyncPost(string url, string requestData, string strAccessToken, SortedDictionary<string, object> headDictionary = null)
        //{
        //    try
        //    {
        //        ResponseResult responseResult = new ResponseResult();
        //        HttpClient httpClient = new HttpClient();
        //        httpClient.SetBearerToken(strAccessToken);
        //        //設置消息頭
        //        if (headDictionary != null && headDictionary.Count > 0)
        //        {
        //            foreach (var item in headDictionary)
        //            {
        //                httpClient.DefaultRequestHeaders.Add(item.Key, item.Value.ToString());
        //            }
        //        }
        //        HttpContent contentPost = new StringContent(requestData, Encoding.UTF8, "application/json");
        //        var response = await httpClient.PostAsync(url, contentPost);
        //        if (!response.IsSuccessStatusCode)
        //        {
        //            responseResult.ErrorMsg = response.StatusCode.ToString();
        //            responseResult.IsSuccess = false;
        //        }
        //        else
        //        {
        //            responseResult.Result = await response.Content.ReadAsStringAsync();
        //        }
        //        return responseResult;
        //    }
        //    catch (Exception ex)
        //    {
        //        string msg = ex.Message;
        //        throw;
        //    }

        //}

    }

    /// <summary>
    /// 
    /// </summary>
    public class ResponResult
    {
        public string api_version { get; set; }
        public bool success { get; set; }
        public string code { get; set; }
        public object message { get; set; }
        public string data { get; set; }
    }

}
