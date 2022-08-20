using JobFilter2.Models;
using JobFilter2.Models.Entities;
using JobFilter2.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger<CrawlSettingController> _logger;

        public CrawlSettingController(JobFilterContext context, ILogger<CrawlSettingController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var data = await _context.CrawlSettings.ToListAsync();
                return View(data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                TempData["message"] = "操作失敗";
                return RedirectToRoute(new { controller = "Home", action = "Index" });
            }
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CrawlSetting data)
        {
            try
            {
                // 新增爬蟲設定 & 寫入DB
                _context.Add(data);
                await _context.SaveChangesAsync();
                TempData["message"] = "新增成功";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                TempData["message"] = "操作失敗";
            }
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Edit(int? id)
        {
            try
            {
                if (id == null)
                {
                    return NotFound();
                }

                var data = await _context.CrawlSettings.FirstOrDefaultAsync(u => u.Id == id);

                if (data == null)
                {
                    return NotFound();
                }

                return View(data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                TempData["message"] = "操作失敗";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CrawlSetting data)
        {
            try
            {
                _context.Entry(data).Property(p => p.TargetUrl).IsModified = true;
                _context.Entry(data).Property(p => p.MinSalary).IsModified = true;
                _context.Entry(data).Property(p => p.Seniority).IsModified = true;
                _context.Entry(data).Property(p => p.Remark).IsModified = true;
                await _context.SaveChangesAsync();
                TempData["message"] = "修改成功";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                TempData["message"] = "操作失敗";
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<string> Delete(CrawlSetting data)
        {
            try
            {
                _context.Entry(data).State = EntityState.Deleted;
                await _context.SaveChangesAsync();
                TempData["message"] = "刪除成功";
                return "刪除成功";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return "操作失敗";
            }
        }

        public async Task<IActionResult> DoCrawl(int? id)
        {
            try
            {
                // 爬取目標頁面，提取工作列表
                var crawlSetting = await _context.CrawlSettings.FirstOrDefaultAsync(u => u.Id == id);
                List<JobItem> jobItems = await crawlService.GetTargetItems(crawlSetting);
                if (jobItems.Count == 0)
                {
                    TempData["message"] = "搜尋到0筆工作";
                    return RedirectToAction("Index");
                }

                // 根據DB資訊來過濾工作列表，並將結果儲存到 Session
                jobItems = await crawlService.GetUnblockedItems(_context, jobItems);
                HttpContext.Session.SetString("jobItems", JsonConvert.SerializeObject(jobItems));

                return RedirectToAction("JobItems");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                TempData["message"] = "操作失敗";
                return RedirectToAction("Index");
            }
        }

        public IActionResult JobItems()
        {
            return View();
        }
    }
}
