using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Http;
using System.IO;

namespace Data
{
    public class WebHandler
    {
        private static readonly HttpClient _httpClient = new();

        private static string GetWebPage(string url)
        {
            return _httpClient.GetStringAsync(url).Result;
        }

        public static HtmlAgilityPack.HtmlDocument? GetWebPageAsHtmlDocument(string url)
        {
            HtmlAgilityPack.HtmlDocument? doc = null;

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
