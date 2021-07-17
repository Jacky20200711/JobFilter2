using JobFilter2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Dom;

namespace JobFilter2.Services
{
    public class CrawlService
    {
        private void GetTargetJobs(IDocument document, List<JobItem> jobItems)
        {
            // 取出夾帶工作訊息的標籤(每一個工作項目都被包含在一對 article 的標籤)
            var itemsCssSelector = document.QuerySelectorAll("article");

            // 走訪每一對標籤，萃取出各自夾帶的工作內容
            foreach (var item in itemsCssSelector)
            {
                IElement JobLink = item.QuerySelector("div h2 a");
                IElement JobAddress = item.QuerySelector("div ul li a");
                IElement JobSalary = item.QuerySelector(".b-tag--default");

                if (JobLink != null && JobAddress != null && JobSalary != null)
                {
                    
                    string Link = "https:" + JobLink.GetAttribute("href");
                    string Title = JobLink.TextContent;
                    string Company = JobAddress.TextContent.Trim();
                    string Address = JobAddress.GetAttribute("title").Split("公司住址：")[1];
                    string Salary = JobSalary.TextContent;

                    jobItems.Add(new JobItem
                    {
                        Link = Link,
                        Title = Title,
                        Company = Company,
                        Address = Address,
                        Salary = Salary
                    });
                }
            }
        }

        private void GetTargetPages(List<Crawler> crawlers, CrawlSetting crawlSetting)
        {
            // 固定爬取十個分頁
            for(int i = 1; i <= 10; i++)
            {
                crawlers.Add(new Crawler());
                _ = crawlers.Last().LoadPage(crawlSetting, i);
            }

            // 等待爬蟲結束任務
            while (crawlers.Any(c => c.GetDomTree() == null))
            {
                Thread.Sleep(200);
            }
        }

        public List<JobItem> GetTargetItems(CrawlSetting crawlSetting)
        {
            // 爬取目標頁面
            List<Crawler> crawlers = new List<Crawler>();
            GetTargetPages(crawlers, crawlSetting);

            // 擷取工作內容
            List<JobItem> jobItems = new List<JobItem>();
            foreach(var crawler in crawlers)
            {
                GetTargetJobs(crawler.GetDomTree(), jobItems);
            }

            return jobItems;
        }
    }
}
