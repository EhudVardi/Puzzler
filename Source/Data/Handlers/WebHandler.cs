using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.IO;

namespace Data
{
    public class WebHandler
    {
        private static string GetWebPage(string url)
        {
            string html = null;
            try
            {
                WebRequest request = WebRequest.Create(url);
                WebResponse response = request.GetResponse();
                Stream data = response.GetResponseStream();
                html = String.Empty;
                using (StreamReader sr = new StreamReader(data))
                {
                    html = sr.ReadToEnd();
                }
            }
            catch (Exception ex)
            {

            }

            return html;
        }

        public static HtmlAgilityPack.HtmlDocument GetWebPageAsHtmlDocument(string url)
        {
            HtmlAgilityPack.HtmlDocument doc = null;

            string data = GetWebPage(url);
            if (data != null)
            {
                doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml(GetWebPage(url));
            }

            return doc;
        }
    }
}
