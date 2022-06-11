using JobFilter2.Models;
using JobFilter2.Models.Entities;
using JobFilter2.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobFilter2.Controllers
{
    public class CrawlSettingController : Controller
    {
        private readonly JobFilterContext _context;
        private readonly CrawlService crawlService = new CrawlService();
        private readonly ILogger<CrawlSettingController> _logger;

        public CrawlSettingController(JobFilterContext context, ILogger<CrawlSettingController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var data = await _context.CrawlSettings.ToListAsync();
            return View(data);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(IFormCollection PostData)
        {
            // 取出各欄位的值
            string remark = PostData["remark"].ToString() ?? null;
            string targetUrl = PostData["targetUrl"].ToString() ?? null;
            string seniority = PostData["seniority"].ToString() ?? null;
            int minSalary = int.Parse(PostData["minSalary"].ToString());

            // 新增爬蟲設定
            CrawlSetting crawlSetting = new CrawlSetting
            {
                Remark = remark,
                TargetUrl = targetUrl,
                MinSalary = minSalary,
                Seniority = seniority,
            };

            _context.Add(crawlSetting);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Edit(int? id)
        {
            #region 檢查此筆資料是否存在

            if (id == null)
            {
                return Content("<h2>資料id錯誤!</h2>", "text/html", Encoding.UTF8);
            }

            var crawlSetting = await _context.CrawlSettings.FirstOrDefaultAsync(u => u.Id == id);

            if (crawlSetting == null)
            {
                return Content("<h2>資料不存在!</h2>", "text/html", Encoding.UTF8);
            }

            #endregion

            return View(crawlSetting);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(IFormCollection PostData)
        {
            // 提取欄位內容
            int id = int.Parse(PostData["Id"].ToString());
            int minSalary = int.Parse(PostData["minSalary"].ToString());
            string remark = PostData["remark"].ToString() ?? null;
            string targetUrl = PostData["targetUrl"].ToString() ?? null;
            string seniority = PostData["seniority"].ToString() ?? null;

            // 取得該筆資料
            var crawlSetting = await _context.CrawlSettings.FirstOrDefaultAsync(u => u.Id == id);
            if (crawlSetting == null)
            {
                return Content("<h2>資料不存在!</h2>", "text/html", Encoding.UTF8);
            }

            // 修改該筆資料
            crawlSetting.Remark = remark;
            crawlSetting.TargetUrl = targetUrl;
            crawlSetting.Seniority = seniority;
            crawlSetting.MinSalary = minSalary;
            await _context.SaveChangesAsync();

            TempData["message"] = "修改成功";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<string> Delete(int? id)
        {
            #region 檢查此筆資料是否存在

            if (id == null)
            {
                return "刪除失敗，查無這筆資料!";
            }

            var crawlSetting = await _context.CrawlSettings.FindAsync(id);

            if (crawlSetting == null)
            {
                return "刪除失敗，查無這筆資料!";
            }

            #endregion

            // 刪除資料並更新DB
            _context.Remove(crawlSetting);
            await _context.SaveChangesAsync();

            return "刪除成功";
        }

        public IActionResult DoCrawl(int? id)
        {
            #region 檢查此筆資料是否存在

            if (id == null)
            {
                return Content("<h2>資料id錯誤!</h2>", "text/html", Encoding.UTF8);
            }

            var crawlSetting = _context.CrawlSettings.FirstOrDefault(u => u.Id == id);

            if (crawlSetting == null)
            {
                return Content("<h2>資料不存在!</h2>", "text/html", Encoding.UTF8);
            }

            #endregion

            // 爬取目標頁面 & 提取工作列表
            List<JobItem> jobItems = crawlService.GetTargetItems(crawlSetting);

            if(jobItems.Count == 0)
            {
                TempData["message"] = "請檢查104網站是否運作正常，或是調整爬蟲的設定!";
                return RedirectToRoute( new { controller = "Home", action = "Index" });
            }

            // 根據DB的黑名單，剔除不想看到的工作
            jobItems = crawlService.GetUnblockedItems(_context, jobItems);
            HttpContext.Session.SetString("jobItems", JsonConvert.SerializeObject(jobItems));

            return RedirectToAction("JobItems");
        }

        public IActionResult JobItems()
        {
            return View();
        }
    }
}
