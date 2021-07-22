using JobFilter2.Models;
using JobFilter2.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JobFilter2.Controllers
{
    public class CrawlSettingController : Controller
    {
        private readonly JobFilterContext _context;
        private readonly CrawlService crawlService = new CrawlService();

        public CrawlSettingController(JobFilterContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.CrawlSettings.ToListAsync());
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
            string targetUrl = PostData["targetUrl"].ToString() ?? null;
            string seniority = PostData["seniority"].ToString() ?? null;
            int minSalary = int.Parse(PostData["minSalary"].ToString());

            // 新增爬蟲設定
            CrawlSetting crawlSetting = new CrawlSetting
            {
                TargetUrl = targetUrl,
                MinSalary = minSalary,
                Seniority = seniority,
            };

            _context.Add(crawlSetting);
            await _context.SaveChangesAsync();

            TempData["message"] = "新增成功";
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Edit(int? id)
        {
            #region 檢查此筆資料是否存在，若不存在則跳轉到錯誤頁面

            if (id == null)
            {
                return NotFound();
            }

            var crawlSetting = await _context.CrawlSettings.FirstOrDefaultAsync(u => u.Id == id);

            if (crawlSetting == null)
            {
                return NotFound();
            }

            #endregion

            return View(crawlSetting);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(IFormCollection PostData)
        {
            // 取出各欄位的值
            int id = int.Parse(PostData["Id"].ToString());
            int minSalary = int.Parse(PostData["minSalary"].ToString());
            string targetUrl = PostData["targetUrl"].ToString() ?? null;
            string seniority = PostData["seniority"].ToString() ?? null;

            // 取得該筆資料
            var crawlSetting = await _context.CrawlSettings.FirstOrDefaultAsync(u => u.Id == id);
            if (crawlSetting == null)
            {
                TempData["message"] = "修改失敗，此筆資料已被刪除";
                return RedirectToAction("Edit", new { id });
            }

            // 修改該筆資料
            crawlSetting.TargetUrl = targetUrl;
            crawlSetting.Seniority = seniority;
            crawlSetting.MinSalary = minSalary;
            await _context.SaveChangesAsync();

            TempData["message"] = "修改成功";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public string Delete(int? id)
        {
            #region 檢查此筆資料是否存在

            if (id == null)
            {
                return "刪除失敗，查無這筆資料!";
            }

            var crawlSetting = _context.CrawlSettings.FirstOrDefault(u => u.Id == id);

            if (crawlSetting == null)
            {
                return "刪除失敗，查無這筆資料!";
            }

            #endregion

            // 刪除用戶並寫入DB
            _context.Remove(crawlSetting);
            _context.SaveChanges();
            return "刪除成功";
        }

        public IActionResult DoCrawl(int? id)
        {
            #region 檢查此筆資料是否存在

            if (id == null)
            {
                return NotFound();
            }

            var crawlSetting = _context.CrawlSettings.FirstOrDefault(u => u.Id == id);

            if (crawlSetting == null)
            {
                return NotFound();
            }

            #endregion

            // 取得爬下來的頁面中，所有的工作項目
            List<JobItem> jobItems = crawlService.GetTargetItems(crawlSetting);

            // 檢查DB的黑名單，過濾已封鎖的工作項目
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
