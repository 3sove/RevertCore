using System;
using System.IO;
using System.Net;
using System.Text;
using System.Web;

namespace Revert.Core.Extensions
{
    public static class WebExtensions
    {
        public static HttpWebResponse GetWebResponse(this HttpWebRequest request)
        {
            var response = request.GetResponse() as HttpWebResponse;
            if (response == null) throw new NullReferenceException("Could not get a response for web request to " + request.RequestUri);
            return response;
        }

        public static string GetText(this HttpWebResponse response)
        {
            string responseText = string.Empty;
            using (var responseStream = response.GetResponseStream())
                if (responseStream != null)
                    using (var streamReader = new StreamReader(responseStream))
                        responseText = streamReader.ReadToEnd();
            return responseText;
        }

        public static string ToHttpWebHtml(this string url, CookieContainer cookieContainer = null)
        {
            if (cookieContainer == null) cookieContainer = new CookieContainer();
            var request = url.ToHttpWebRequest(cookieContainer);
            string result = "";
            using (var response = request.GetResponse())
            {
                using (var stream = response.GetResponseStream())
                using (var newStream = new MemoryStream())
                {
                    if (stream == null)
                        return result;
                    stream.CopyTo(newStream);
                    newStream.Position = 0;

                    using (var streamReader = new StreamReader(newStream))
                    {
                        result = streamReader.ReadToEnd();
                        newStream.Position = 0;
                    }
                }
            }
            return result;
        }


        public static HttpWebRequest ToHttpWebRequest(this string url)
        {
            var request = WebRequest.Create(url) as HttpWebRequest;
            if (request == null) throw new NullReferenceException("Couldn't create basic HttpWebRequest to " + url);
            PopulateRequest(request);
            return request;
        }


        public static HttpWebRequest ToHttpWebRequest(this string url, CookieContainer cookieContainer)
        {
            var request = WebRequest.Create(url) as HttpWebRequest;
            if (request == null) throw new NullReferenceException("Couldn't create basic HttpWebRequest to " + url);
            PopulateRequest(request);
            request.CookieContainer = cookieContainer;
            return request;
        }


        public static HttpWebRequest ToHttpWebRequest(this string url, CookieContainer cookieContainer, string requestMethod)
        {
            var request = WebRequest.Create(url) as HttpWebRequest;
            if (request == null) throw new NullReferenceException("Couldn't create basic HttpWebRequest to " + url);
            PopulateRequest(request);
            request.CookieContainer = cookieContainer;
            return request;
        }

        public static void PopulateRequest(this HttpWebRequest request, string requestMethod = "GET")
        {
            request.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
            request.Method = requestMethod;
            //request.ContentType = "application/x-www-form-urlencoded";
            request.Accept = "text/html, application/xml, application/xhtml+xml, */*";
            request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML";
            //request.UserAgent = "Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.2; WOW64; Trident/6.0)";

            request.Host = request.RequestUri.Host;
            request.KeepAlive = true;
            request.Headers.Add(HttpRequestHeader.AcceptLanguage, "en-US");
            request.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip, deflate");
            //request.Headers.Add(HttpRequestHeader.CacheControl, "no-cache");
        }

        public static void AddData(this HttpWebRequest request, string dataToAdd)
        {
            var data = Encoding.UTF8.GetBytes(dataToAdd);
            request.ContentLength = data.Length;
            using (var stream = request.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);
            }
        }

        private const string SpanBeginning = "<span class='search_Highlighting_1'>";
        private const string SpanEnd = "</span>";
        public static string HilightSearchTerm_HTML(this string stringToConvert, string searchTerm)
        {
            var strUpper = stringToConvert.ToUpper();
            searchTerm = searchTerm.ToUpper();
            if (strUpper.Contains(searchTerm) == false) return stringToConvert;

            var position = 0;
            var searchTermLength = searchTerm.Length;
            var sb = new StringBuilder();

            while (true)
            {
                if (strUpper.Substring(position).Contains(searchTerm) == false)
                {
                    sb.Append(stringToConvert.Substring(position));
                    break;
                }

                var previousPosition = position;
                position += strUpper.Substring(position).IndexOf(searchTerm);

                sb.Append(stringToConvert.Substring(previousPosition, position - previousPosition));
                sb.Append(SpanBeginning);
                sb.Append(stringToConvert.Substring(position, searchTermLength));
                sb.Append(SpanEnd);

                position += searchTermLength;
            }

            return sb.ToString();
        }
    }
}