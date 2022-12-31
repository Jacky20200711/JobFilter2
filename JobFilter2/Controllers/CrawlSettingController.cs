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
                return Content("ERROR");
            }
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CrawlSetting data, string seniority1, string seniority2, string seniority3)
        {
            try
            {
                // 調整年資欄位
                List<string> sList = new List<string>();
                if (!string.IsNullOrEmpty(seniority1)) sList.Add(seniority1);
                if (!string.IsNullOrEmpty(seniority2)) sList.Add(seniority2);
                if (!string.IsNullOrEmpty(seniority3)) sList.Add(seniority3);
                data.Seniority = string.Join(",", sList);

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
        public async Task<IActionResult> Edit(CrawlSetting data, string seniority1, string seniority2, string seniority3)
        {
            try
            {
                // 調整年資欄位
                List<string> sList = new List<string>();
                if (!string.IsNullOrEmpty(seniority1)) sList.Add(seniority1);
                if (!string.IsNullOrEmpty(seniority2)) sList.Add(seniority2);
                if (!string.IsNullOrEmpty(seniority3)) sList.Add(seniority3);
                data.Seniority = string.Join(",", sList);

                // 設定欲修改的欄位
                _context.Entry(data).Property(p => p.Remark).IsModified = true;
                _context.Entry(data).Property(p => p.TargetUrl).IsModified = true;
                _context.Entry(data).Property(p => p.MinSalary).IsModified = true;
                _context.Entry(data).Property(p => p.Seniority).IsModified = true;
                _context.Entry(data).Property(p => p.ExcludeWords).IsModified = true;
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

                // 若職缺數量為零，視為搜尋失敗
                if (jobItems.Count == 0)
                {
                    TempData["message"] = "搜尋失敗";
                    return RedirectToAction("Index");
                }

                // 過濾掉已封鎖的工作和公司
                jobItems = await crawlService.GetUnblockedItems(_context, jobItems);

                // 過濾掉職稱裡面含有特定關鍵字的職缺
                jobItems = crawlService.FilterByExcludeWords(jobItems, crawlSetting.ExcludeWords);

                // 將最終結果儲存到 Session
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
            List<JobItem> jobItems = new List<JobItem>();
            string itemStr = HttpContext.Session.GetString("jobItems");
            if (itemStr != null)
            {
                jobItems = JsonConvert.DeserializeObject<List<JobItem>>(itemStr);
            }

            return View(jobItems);
        }
    }
}
