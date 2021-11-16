using JobFilter2.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using AngleSharp.Dom;

namespace JobFilter2.Services
{
    public class CrawlService
    {
        private void GetTargetJobs(IDocument document, List<JobItem> jobItems)
        {
            if (document == null) return;

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
                    string Code = item.GetAttribute("data-job-no");
                    string Link = "https:" + JobLink.GetAttribute("href");
                    string Title = JobLink.TextContent;
                    string Company = JobAddress.TextContent.Trim();
                    string Address = JobAddress.GetAttribute("title").Split("公司住址：")[1];
                    string Salary = JobSalary.TextContent;

                    jobItems.Add(new JobItem
                    {
                        Code = Code,
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
            int loop = 0;
            while (crawlers.Any(c => c.GetDomTree() == null))
            {
                Thread.Sleep(200);

                // 如果等太久則結束爬取
                if (loop++ == 25) break;
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

        /// <summary>
        /// 根據DB資訊來過濾工作項目
        /// </summary>
        public List<JobItem> GetUnblockedItems(JobFilterContext context, List<JobItem> jobItems)
        {
            var blockJobItems = context.BlockJobItems.ToList();
            var blockCompanys = context.BlockCompanies.ToList();

            HashSet<string> blockJobCodeSet = new HashSet<string>();
            HashSet<string> blockCompanySet = new HashSet<string>();

            // 取得已封鎖的工作代碼
            foreach (var b in blockJobItems)
            {
                blockJobCodeSet.Add(b.JobCode);
            }

            // 取得已封鎖的公司名稱
            foreach (var b in blockCompanys)
            {
                blockCompanySet.Add(b.CompanyName);
            }

            // 取出沒有被過濾掉的項目(有些工作偶爾會重複，若遇到 jobCode 重複則直接覆蓋舊的)
            Dictionary<string, JobItem> jobDict = new Dictionary<string, JobItem>();
            foreach (var jobItem in jobItems)
            {
                if(!blockJobCodeSet.Contains(jobItem.Code) && !blockCompanySet.Contains(jobItem.Company))
                {
                    jobDict[jobItem.Code] = jobItem;
                }
            }

            // 取出已過濾且未重複的工作
            List<JobItem> new_jobitems = new List<JobItem>();
            foreach(var item in jobDict)
            {
                new_jobitems.Add(item.Value);
            }

            return new_jobitems;
        }

        /// <summary>
        /// 排除掉指定的工作項目或公司名稱
        /// </summary>
        public List<JobItem> GetUpdateList(List<JobItem> jobItems, string target, string blockType)
        {
            List<JobItem> new_jobitems = new List<JobItem>();

            if (blockType == "jobCode")
            {
                foreach (var jobItem in jobItems)
                {
                    if(jobItem.Code != target)
                    {
                        new_jobitems.Add(jobItem);
                    }
                }
            }
            else if(blockType == "company")
            {
                foreach (var jobItem in jobItems)
                {
                    if (jobItem.Company != target)
                    {
                        new_jobitems.Add(jobItem);
                    }
                }
            }

            return new_jobitems;
        }
    }
}
