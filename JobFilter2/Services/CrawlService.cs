using JobFilter2.Models;
using System.Collections.Generic;
using AngleSharp.Dom;
using System.Threading.Tasks;
using NLog;
using System;
using AngleSharp;
using JobFilter2.Models.Entities;
using System.Net.Http;
using Azure;
using System.Net.Http.Json;

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

            // 檢查是否排除面議，調整爬取網址 => 以年薪查詢時，強制排除面議
            targetUrl = targetUrl.Replace("&scneg=0", "");
            targetUrl = targetUrl.Replace("&scneg=1", "");
            targetUrl += crawlSetting.HasSalary == "是" || salaryType == "Y" ? "&scneg=0" : "&scneg=1";

            try
            {
                // 紀錄以年薪爬取的網址，以及以月薪爬取的網址(只抓第一頁)
                if(salaryType == "Y" || currentPage == 1)
                    _logger.Debug($"minSalary = {minSalary} & 網址 : {targetUrl}");

                // 將網址前綴置換為API網址
                targetUrl = targetUrl.Replace("https://www.104.com.tw/jobs/search/", "https://www.104.com.tw/jobs/search/api/jobs");

                // 偽造請求來源，避免 403 錯誤 
                httpClient.DefaultRequestHeaders.Add("Origin", "https://www.104.com.tw");

                // 送出請求
                var responseMessage = await httpClient.GetAsync(targetUrl);

                // 查看結果
                if (responseMessage.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    // 將 JSON 轉 Class
                    pageData.JobRoot = await responseMessage.Content.ReadFromJsonAsync<JobRoot>();
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
        /// 提取工作明細
        /// </summary>
        private void GetTargetJobs(JobRoot jobRoot, List<JobItem> jobItems, HashSet<string> jobCodeSet)
        {
            if (jobRoot == null) return;

            try
            {
                // 檢查工作明細
                foreach (var item in jobRoot.Data)
                {
                    string Code = item.JobNo;
                    string Link = item.Link.Job.Replace("\\", "");
                    string Title = $"{item.JobName}".Trim();
                    string Company = $"{item.CustName}".Trim();
                    string Address = $"{item.JobAddrNoDesc}{item.JobAddress}".Trim();
                    int salaryLow = item.SalaryLow == 0 ? 40000 : item.SalaryLow;
                    int salaryHigh = item.SalaryHigh == 0 ? 40000 : item.SalaryHigh;
                    string Salary = $"{salaryLow}~{salaryHigh}";
                    if (item.SalaryLow == 0 && item.SalaryHigh == 0)
                        Salary = "待遇面議";

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
            catch (Exception ex) 
            {
                _logger.Error($"{ex.Message}\n{ex.StackTrace}");
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
            int lastPage = 10;
            int loopRemainTimes = 2; // 最多爬 10 * 2 = 20 個頁面就好，降低拿到 429 錯誤的機率

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
                GetTargetJobs(pageData.JobRoot, jobItems, jobCodeSet);
            }

            return jobItems;
        }
    }
}
