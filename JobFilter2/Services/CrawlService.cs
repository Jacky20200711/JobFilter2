﻿using JobFilter2.Models;
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

namespace JobFilter2.Services
{
    public class CrawlService
    {
        private static readonly HttpClient httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// 派出爬蟲，爬取指定的網址
        /// </summary>
        public async Task LoadPage(Crawler crawler, CrawlSetting crawlSetting, int currentPage = 1, string sctp = "M")
        {
            // 最低年薪視為(月薪下限*14)
            int minSalary = sctp == "Y" ? crawlSetting.MinSalary * 14 : crawlSetting.MinSalary;

            // 若沒有設置 scstrict = 1 則薪資過濾會失效
            string targetUrl = crawlSetting.TargetUrl + $"&sctp={sctp}&scmin={minSalary}&page={currentPage}&jobexp={crawlSetting.Seniority}&scstrict=1";

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
                    crawler.domTree = await context.OpenAsync(res => res.Content(pageContent));
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
            List<Crawler> crawlers = new List<Crawler>();

            // 製造多個爬蟲，爬取目標分頁
            for (int i = 1; i <= 20; i++)
            {
                crawlers.Add(new Crawler());
                _ = LoadPage(crawlers[^1], crawlSetting, i);
            }

            // 寫明年薪的工作比較少，爬取第一頁即可
            crawlers.Add(new Crawler());
            _ = LoadPage(crawlers[^1], crawlSetting, 1, sctp:"Y");

            // 等待所有爬蟲結束任務
            while (crawlers.Any(c => !c.isMissionCompleted))
            {
                await Task.Delay(200);
            }

            // 依序提取各分頁的工作資訊
            HashSet<string> jobCodeSet = new HashSet<string>(); // 儲存出現過的 jobCode (用來避免相同的職缺重複出現)
            List<JobItem> jobItems = new List<JobItem>();
            foreach(var crawler in crawlers)
            {
                GetTargetJobs(crawler.domTree, jobItems, jobCodeSet);
            }

            return jobItems;
        }

        /// <summary>
        /// 根據DB儲存的黑名單，來過濾傳入的 jobItems
        /// </summary>
        /// <returns>過濾完畢的工作列表</returns>
        public async Task<List<JobItem>> GetUnblockedItems(JobFilterContext context, List<JobItem> jobItems)
        {
            List<JobItem> new_jobitems = new List<JobItem>();
            try
            {
                // 取得已封鎖的工作代碼與公司名稱
                var blockJobItems = await context.BlockJobItems.ToListAsync();
                var blockCompanys = await context.BlockCompanies.ToListAsync();

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
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 刷新 Session 儲存的工作列表(當封鎖工作或封鎖公司之後，都會Call這個函數)
        /// </summary>
        public void UpdateJobList(string target, string blockType, HttpContext httpContext)
        {
            List<JobItem> new_jobitems = new List<JobItem>();

            try
            {
                // 從 Session 取出工作列表
                string jobItemsStr = httpContext.Session.GetString("jobItems");
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
                httpContext.Session.SetString("jobItems", JsonConvert.SerializeObject(new_jobitems));
            }
            catch(Exception)
            {
                throw;
            }
        }

        public List<JobItem> FilterByExcludeWords(List<JobItem> jobItems, string excludeWords)
        {
            if (string.IsNullOrEmpty(excludeWords))
            {
                return jobItems;
            }

            try
            {
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
            catch(Exception)
            {
                throw;
            }
        }
    }
}
