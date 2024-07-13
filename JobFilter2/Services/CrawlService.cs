using JobFilter2.Models;
using System.Collections.Generic;
using AngleSharp.Dom;
using System.Threading.Tasks;
using NLog;
using System;
using AngleSharp;
using JobFilter2.Models.Entities;
using System.Net.Http;

namespace JobFilter2.Services
{
    public class CrawlService
    {
        private static readonly HttpClient httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// 爬取指定的網址與分頁
        /// </summary>
        public async Task LoadPage(PageData pageData, CrawlSetting crawlSetting, int currentPage = 1, string salaryType = "M")
        {
            // 若以年薪判斷，則將下限設為80萬
            int minSalary = salaryType == "Y" ? 800000 : crawlSetting.MinSalary;

            // 若沒有設置 scstrict = 1 則薪資過濾會失效
            string targetUrl = crawlSetting.TargetUrl + $"&sctp={salaryType}&scmin={minSalary}&page={currentPage}&jobexp={crawlSetting.Seniority}&scstrict=1";

            // 檢查是否排除面議，調整爬取網址
            targetUrl = targetUrl.Replace("&scneg=0", "");
            targetUrl = targetUrl.Replace("&scneg=1", "");
            targetUrl += crawlSetting.HasSalary == "是" ? "&scneg=0" : "&scneg=1";

            try
            {
                // 紀錄以年薪爬取的網址，以及以月薪爬取的網址(只抓第一頁)
                if(salaryType == "Y" || currentPage == 1)
                    _logger.Debug($"minSalary = {minSalary} & 網址 : {targetUrl}");

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
        /// 異步爬取多個頁面，並且提取各項工作訊息
        /// </summary>
        /// <returns>提取出來的工作列表</returns>
        public async Task<List<JobItem>> GetTargetItems(CrawlSetting crawlSetting)
        {
            List<PageData> pageDataList = new List<PageData>();

            int firstPage = 1;
            int lastPage = 15;
            int loopRemainTimes = 2; // 最多爬 15 * 2 = 30 個頁面就好，不然會拿到 429 錯誤代碼(請求太多)

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
                await Task.Delay(500); // 稍微停止一下再繼續爬下一批，降低被限流或黑名單的風險

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
    }
}
