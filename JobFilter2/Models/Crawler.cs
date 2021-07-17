using AngleSharp;
using AngleSharp.Dom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace JobFilter2.Models
{
    public class Crawler
    {
        private readonly HttpClient httpClient = new HttpClient();
        private IDocument domTree = null;

        public async Task LoadPage(CrawlSetting crawlSetting, int currentPage = 1)
        {
            int seniority = crawlSetting.Seniority switch
            {
                "1年以下" => 1,
                "1~3年" => 3,
                "3~5年" => 5,
                _ => 1,
            };

            string targetUrl = crawlSetting.TargetUrl + $"&scmin={crawlSetting.MinSalary}&page={currentPage}&jobexp={seniority}";

            var responseMessage = await httpClient.GetAsync(targetUrl);

            if (responseMessage.StatusCode == System.Net.HttpStatusCode.OK)
            {
                // 取得頁面內容
                string pageContent = responseMessage.Content.ReadAsStringAsync().Result;

                // 將頁面內容轉成 domTree 的形式
                var config = Configuration.Default;
                var context = BrowsingContext.New(config);
                domTree = await context.OpenAsync(res => res.Content(pageContent));
            }
        }

        public IDocument GetDomTree()
        {
            return domTree;
        }
    }
}
