using System;
using System.Net;

namespace Revert.Core.IO
{
    public class WebCrawler
    {
        public static HttpWebRequest GenerateHttpWebRequest(string url, CookieContainer cookieContainer, HttpRequestMethods method = HttpRequestMethods.Get)
        {
            var searchRequest = WebRequest.Create(url) as HttpWebRequest;
            if (searchRequest == null) throw new NullReferenceException("Couldn't create basic HttpWebRequest to " + url);
            PopulateHttpWebRequest(searchRequest, method == HttpRequestMethods.Get ? "GET" : "POST");
            searchRequest.CookieContainer = cookieContainer;
            return searchRequest;
        }

        public static void PopulateHttpWebRequest(HttpWebRequest request, string requestMethod = "GET")
        {
            request.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
            request.Method = requestMethod;
            request.Accept = "text/html, application/xml, application/xhtml+xml, */*";
            request.UserAgent = "Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.2; WOW64; Trident/6.0)";
            //request.Host = request.RequestUri.Host;
            request.KeepAlive = true;
            request.Headers.Add(HttpRequestHeader.AcceptLanguage, "en-US,en;q=0.8");
            request.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip, deflate");
        }
    }
}
