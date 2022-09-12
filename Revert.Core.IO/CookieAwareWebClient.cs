using System;
using System.Net;

namespace Revert.Core.IO
{
    public class CookieAwareWebClient : WebClient
    {
        public CookieContainer CookieContainer { get; set; }
        public Uri Uri { get; set; }

        public CookieAwareWebClient()
            : this(new CookieContainer())
        {
        }

        public CookieAwareWebClient(CookieContainer cookies)
        {
            this.CookieContainer = cookies;
        }

        protected override WebRequest GetWebRequest(Uri address)
        {
            HttpWebRequest request = base.GetWebRequest(address) as HttpWebRequest;
            if (request == null) return null;
            request.CookieContainer = CookieContainer;
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            request.Accept = "text/html, application/xml, application/xhtml+xml, */*";
            request.UserAgent = "Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.2; WOW64; Trident/6.0)";
            request.Host = request.RequestUri.Host;
            request.KeepAlive = true;
            request.Headers.Add(HttpRequestHeader.AcceptLanguage, "en-US,en;q=0.8");
            request.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip, deflate");

            return request;
        }

        protected override WebResponse GetWebResponse(WebRequest request)
        {
            WebResponse response = base.GetWebResponse(request);

            //string setCookieHeader = response.Headers[HttpResponseHeader.SetCookie];

            //if (setCookieHeader != null)
            //{
            //    //do something if needed to parse out the cookie.
            //    Cookie cookie = new Cookie(); //create cookie
            //    CookieContainer.Add(cookie);
            //}
            return response;
        }
    }
}