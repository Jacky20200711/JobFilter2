using JobFilter2.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using AngleSharp.Dom;
using System.Threading.Tasks;
using NLog;
using System;
using AngleSharp;
using JobFilter2.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace JobFilter2.Services
{
    public class CrawlService
    {
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// 派出爬蟲，爬取指定的網址
        /// </summary>
        public async Task LoadPage(Crawler crawler, CrawlSetting crawlSetting, int currentPage = 1)
        {
            int seniority = crawlSetting.Seniority switch
            {
                "1年以下" => 1,
                "1~3年" => 3,
                "3~5年" => 5,
                _ => 1,
            };

            string targetUrl = crawlSetting.TargetUrl + $"&scmin={crawlSetting.MinSalary}&page={currentPage}&jobexp={seniority}";

            try
            {
                // 設定逾時
                crawler.httpClient.Timeout = TimeSpan.FromSeconds(15);

                // 送出請求
                var responseMessage = await crawler.httpClient.GetAsync(targetUrl);

                // 查看結果
                if (responseMessage.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    // 取得頁面內容
                    string pageContent = responseMessage.Content.ReadAsStringAsync().Result;

                    // 將頁面內容轉成 domTree 的形式
                    var config = Configuration.Default;
                    var context = BrowsingContext.New(config);
                    crawler.domTree = await context.OpenAsync(res => res.Content(pageContent));
                }
                else
                {
                    _logger.Error($"錯誤代碼 = {(int)responseMessage.StatusCode} & currentPage = {currentPage}");
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                /* 這個變數用來判斷爬蟲是否已經完成工作，
                 * 不論爬蟲執行成功、失敗、或是中途出錯，最後都必須令這個值為 true */
                crawler.isMissionCompleted = true;
            }
        }

        /// <summary>
        /// 解析爬下來的 document 並將提取後的工作放入 jobItems
        /// </summary>
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

        /// <summary>
        /// 爬取目標頁面，提取工作列表 
        /// </summary>
        /// <returns>提取後的工作列表(尚未經由DB資訊做過濾)</returns>
        public List<JobItem> GetTargetItems(CrawlSetting crawlSetting)
        {
            List<Crawler> crawlers = new List<Crawler>();

            // 爬取目標分頁
            for (int i = 1; i <= 20; i++)
            {
                crawlers.Add(new Crawler());
                _ = LoadPage(crawlers[^1], crawlSetting, i);
            }

            // 等待所有爬蟲結束任務
            while (crawlers.Any(c => !c.isMissionCompleted))
            {
                Thread.Sleep(200);
            }

            // 提取每一筆工作的資訊
            List<JobItem> jobItems = new List<JobItem>();
            foreach(var crawler in crawlers)
            {
                GetTargetJobs(crawler.domTree, jobItems);
            }

            return jobItems;
        }

        /// <summary>
        /// 根據DB資訊來過濾傳入的 jobItems
        /// </summary>
        /// <returns>過濾完畢的工作列表</returns>
        public async Task<List<JobItem>> GetUnblockedItems(JobFilterContext context, List<JobItem> jobItems)
        {
            List<JobItem> new_jobitems = new List<JobItem>();
            try
            {
                var blockJobItems = await context.BlockJobItems.ToListAsync();
                var blockCompanys = await context.BlockCompanies.ToListAsync();

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

                // 另存沒有被過濾掉的工作(有些工作偶爾會重複，若遇到 jobCode 重複則直接覆蓋舊的)
                Dictionary<string, JobItem> jobDict = new Dictionary<string, JobItem>();
                foreach (var jobItem in jobItems)
                {
                    if (!blockJobCodeSet.Contains(jobItem.Code) && !blockCompanySet.Contains(jobItem.Company))
                    {
                        jobDict[jobItem.Code] = jobItem;
                    }
                }

                // 取出過濾完畢的工作列表
                foreach (var item in jobDict)
                {
                    new_jobitems.Add(item.Value);
                }
                return new_jobitems;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 從目前的工作列表中，排除指定條件的工作項目(當封鎖工作或封鎖公司之後，都會Call這個函數)
        /// </summary>
        /// <returns>過濾完畢的工作列表</returns>
        public List<JobItem> GetUpdateList(List<JobItem> jobItems, string target, string blockType)
        {
            List<JobItem> new_jobitems = new List<JobItem>();

            if(jobItems == null)
            {
                return new_jobitems;
            }

            // 判斷 User 是要封鎖工作還是封鎖公司
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
