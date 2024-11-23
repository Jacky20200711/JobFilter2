using AngleSharp.Dom;

namespace JobFilter2.Models
{
    public class PageData
    {
        public IDocument document = null;

        public JobRoot JobRoot { get; set; }
    }
}
