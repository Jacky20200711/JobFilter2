using AngleSharp.Dom;
using System.Net.Http;

namespace JobFilter2.Models
{
    public class Crawler
    {
        public HttpClient httpClient = new HttpClient();
        public IDocument domTree = null;
        public bool isMissionCompleted = false;
    }
}
