using AngleSharp;
using AngleSharp.Dom;
using NLog;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace JobFilter2.Models
{
    public class Crawler
    {
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly HttpClient httpClient = new HttpClient();
        public IDocument domTree = null;
        public bool isMissionCompleted = false;

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

            try
            {
                // 設定逾時
                httpClient.Timeout = TimeSpan.FromSeconds(15);

                // 送出請求
                var responseMessage = await httpClient.GetAsync(targetUrl);

                // 查看結果
                if (responseMessage.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    // 取得頁面內容
                    string pageContent = responseMessage.Content.ReadAsStringAsync().Result;

                    // 將頁面內容轉成 domTree 的形式
                    var config = Configuration.Default;
                    var context = BrowsingContext.New(config);
                    domTree = await context.OpenAsync(res => res.Content(pageContent));
                }
                else
                {
                    _logger.Error($"錯誤代碼 = {(int)responseMessage.StatusCode} & currentPage = {currentPage}");
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
            }
            finally
            {
                /* 這個變數用來判斷爬蟲是否已經完成工作，
                 * 所以不論爬蟲執行成功、失敗、或是中途出錯，最後都必須令這個值為 true */
                isMissionCompleted = true;
            }
        }
    }
}
