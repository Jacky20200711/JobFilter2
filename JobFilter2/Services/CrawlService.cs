using JobFilter2.Models;
using System.Collections.Generic;
using System.Linq;
using AngleSharp.Dom;
using System.Threading.Tasks;
using NLog;
using System;
using AngleSharp;
using JobFilter2.Models.Entities;
using Microsoft.EntityFrameworkCore;
using System.Net.Http;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using AngleSharp.Text;

namespace JobFilter2.Services
{
    public class CrawlService
    {
        private static readonly HttpClient httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly JobFilterContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CrawlService(JobFilterContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// 爬取指定的網址與分頁
        /// </summary>
        public async Task LoadPage(PageData pageData, CrawlSetting crawlSetting, int currentPage = 1, string salaryType = "M")
        {
            // 最低年薪視為(月薪下限*14)
            int minSalary = salaryType == "Y" ? crawlSetting.MinSalary * 14 : crawlSetting.MinSalary;

            // 若沒有設置 scstrict = 1 則薪資過濾會失效
            string targetUrl = crawlSetting.TargetUrl + $"&sctp={salaryType}&scmin={minSalary}&page={currentPage}&jobexp={crawlSetting.Seniority}&scstrict=1";

            // 檢查是否排除面議，調整爬取網址
            targetUrl = targetUrl.Replace("&scneg=0", "");
            targetUrl = targetUrl.Replace("&scneg=1", "");
            targetUrl += crawlSetting.HasSalary == "是" ? "&scneg=0" : "&scneg=1";

            try
            {
                // 送出請求
                var responseMessage = await httpClient.GetAsync(targetUrl);

                // 查看結果
                if (responseMessage.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    // 取得頁面內容
                    string pageContent = await responseMessage.Content.ReadAsStringAsync();

                    // 將頁面內容轉成 domTree 的形式
                    var config = Configuration.Default;
                    var context = BrowsingContext.New(config);
                    pageData.document = await context.OpenAsync(res => res.Content(pageContent));
                }
                else
                {
                    _logger.Error($"錯誤代碼 = {(int)responseMessage.StatusCode} & currentPage = {currentPage}");
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"爬蟲出錯 & errorCatch = {ex.Message}\n{ex.StackTrace}");
            }
        }

        /// <summary>
        /// 解析爬下來的 document 並將提取後的工作放入 jobItems
        /// </summary>
        private void GetTargetJobs(IDocument document, List<JobItem> jobItems, HashSet<string> jobCodeSet)
        {
            if (document == null) return;

            // 取出夾帶工作訊息的標籤(每一個工作項目都被包含在一對 article 的標籤)
            var itemsCssSelector = document.QuerySelectorAll("article");

            // 走訪每一對標籤，萃取出各自夾帶的工作內容
            foreach (var item in itemsCssSelector)
            {
                // 若遇到頁面下方的推薦工作，則忽略並停止萃取
                if(item.GetAttribute("class").Contains("recommend"))
                {
                    break;
                }

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

                    // 檢查工作代碼是否曾經出現過，若沒有則添加，否則忽略這筆資料
                    // 根據測試，兩個相同職缺的 jobCode 會一樣，但 jobLink 卻可能不一樣，所以必須用 jobCode 來判斷是否重複
                    if (!jobCodeSet.Contains(Code))
                    {
                        jobCodeSet.Add(Code);
                    }
                    else continue;

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
        public async Task<List<JobItem>> GetTargetItems(CrawlSetting crawlSetting)
        {
            List<PageData> pageDataList = new List<PageData>();

            int firstPage = 1;
            int lastPage = 20;
            int loopRemainTimes = 2;

            while (loopRemainTimes > 0)
            {
                List<Task> tasks = new List<Task>();

                for (int i = firstPage; i <= lastPage; i++)
                {
                    // 新增 PageData (用來儲存這一輪爬蟲取得的頁面內容)
                    pageDataList.Add(new PageData());

                    // 新增爬蟲，爬取目標分頁並將頁面內容轉存到 PageData
                    // 注意，只有非同步函數才可以直接添加到 Task 並自動執行
                    tasks.Add(LoadPage(pageDataList[^1], crawlSetting, i));
                }

                // 派出第一批爬蟲之後，另外再派一隻去爬有註明年薪的工作
                // 由於有註明年薪的工作數量很少，所以爬取一個分頁即可
                if (firstPage == 1)
                {
                    pageDataList.Add(new PageData());
                    tasks.Add(LoadPage(pageDataList[^1], crawlSetting, 1, salaryType: "Y"));
                }

                // 等待所有 Task 結束
                await Task.WhenAll(tasks);

                // 修改分頁範圍，準備進入下一輪迴圈來爬取該範圍的分頁
                firstPage = lastPage + 1;
                lastPage += lastPage;
                loopRemainTimes--;
            }

            // 依序提取各分頁的工作資訊
            HashSet<string> jobCodeSet = new HashSet<string>(); // 儲存出現過的 jobCode (用來避免相同的職缺重複出現)
            List<JobItem> jobItems = new List<JobItem>();
            foreach(var pageData in pageDataList)
            {
                GetTargetJobs(pageData.document, jobItems, jobCodeSet);
            }

            return jobItems;
        }

        /// <summary>
        /// 根據DB的封鎖工作與封鎖公司，來過濾傳入的 jobItems
        /// </summary>
        /// <returns>返回過濾後的結果</returns>
        public async Task<List<JobItem>> GetUnblockedItems(List<JobItem> jobItems)
        {
            List<JobItem> new_jobitems = new List<JobItem>();

            // 取得已封鎖的工作代碼與公司名稱
            var blockJobItems = await _context.BlockJobItems.ToListAsync();
            var blockCompanys = await _context.BlockCompanies.ToListAsync();

            // 將已封鎖的工作代碼和公司名稱，轉存到 HashTable 以加速搜尋比對
            HashSet<string> blockJobCodeSet = new HashSet<string>();
            HashSet<string> blockCompanySet = new HashSet<string>();
            blockJobItems.ForEach(x => blockJobCodeSet.Add(x.JobCode));
            blockCompanys.ForEach(x => blockCompanySet.Add(x.CompanyName));

            // 檢查傳入的工作列表，取出沒有被過濾的工作
            foreach (var jobItem in jobItems)
            {
                if (!blockJobCodeSet.Contains(jobItem.Code) && !blockCompanySet.Contains(jobItem.Company))
                {
                    new_jobitems.Add(jobItem);
                }
            }

            return new_jobitems;
        }

        /// <summary>
        /// 刷新 Session 儲存的工作列表(當封鎖工作或封鎖公司之後，都會Call這個函數)
        /// </summary>
        public void UpdateJobList(string target, string blockType)
        {
            List<JobItem> new_jobitems = new List<JobItem>();

            // 從 Session 取出工作列表
            string jobItemsStr = _httpContextAccessor.HttpContext.Session.GetString("jobItems");
            if (jobItemsStr == null)
            {
                return;
            }

            List<JobItem> jobItems = JsonConvert.DeserializeObject<List<JobItem>>(jobItemsStr);

            // 判斷 User 是要封鎖工作還是封鎖公司
            if (blockType == "jobCode")
            {
                // 觀察工作列表，取出未被封鎖的工作項目
                foreach (var jobItem in jobItems)
                {
                    if (jobItem.Code != target)
                    {
                        new_jobitems.Add(jobItem);
                    }
                }
            }
            else if (blockType == "company")
            {
                // 觀察工作列表，取出未被封鎖的工作項目
                foreach (var jobItem in jobItems)
                {
                    if (jobItem.Company != target)
                    {
                        new_jobitems.Add(jobItem);
                    }
                }
            }

            // 刷新 Session 儲存的工作列表
            _httpContextAccessor.HttpContext.Session.SetString("jobItems", JsonConvert.SerializeObject(new_jobitems));
        }

        /// <summary>
        /// 過濾掉職稱裡面含有特定關鍵字的職缺
        /// </summary>
        /// <returns>返回過濾後的結果</returns>
        public List<JobItem> FilterByExcludeWords(List<JobItem> jobItems, string excludeWords)
        {
            if (string.IsNullOrEmpty(excludeWords))
            {
                return jobItems;
            }

            List<JobItem> new_jobItems = new List<JobItem>();
            List<string> exWords = excludeWords.Split(',').ToList();

            foreach (var job in jobItems)
            {
                // 檢查職稱是否包含關鍵字
                bool hasWord = false;
                foreach (string word in exWords)
                {
                    // 考慮到英文單字，兩邊都轉成小寫再進行比對
                    // 添加 Trim 可以忽略多餘的空白
                    if (job.Title.ToLower().Contains(word.Trim().ToLower()))
                    {
                        hasWord = true;
                        break;
                    }
                }

                if (!hasWord)
                {
                    new_jobItems.Add(job);
                }
            }
            return new_jobItems;
        }

        /// <summary>
        /// 過濾掉最高月薪開太低的職缺
        /// </summary>
        /// <returns>返回過濾後的結果</returns>
        public List<JobItem> FilterByMaxSalary(List<JobItem> jobItems, int maxSalaryOfSetting)
        {
            List<JobItem> new_jobs = new List<JobItem>();

            foreach (var job in jobItems)
            {
                // 如果沒有薪水範圍，視為符合設定並直接添加
                if (!job.Salary.Contains('~'))
                {
                    new_jobs.Add(job);
                    continue;
                }

                // 取出薪水範圍 '~' 右側的數字
                string rightPart = job.Salary.Split('~')[1];
                List<char> digits = new List<char>();
                foreach (char c in rightPart)
                {
                    if (c.IsDigit())
                    {
                        digits.Add(c);
                    }
                }

                // 將這些數字重新拼接，並轉成該職缺的最高月薪
                string digitToStr = new string(digits.ToArray());
                int maxSalaryOfJobItem = int.Parse(digitToStr);

                // 若最高月薪符合設定，則添加這筆資料
                if(maxSalaryOfJobItem >= maxSalaryOfSetting)
                {
                    new_jobs.Add(job);
                }
            }

            return new_jobs;
        }
    }
}
