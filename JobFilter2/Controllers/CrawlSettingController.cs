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
using System.Text;
using System.Threading.Tasks;

namespace JobFilter2.Controllers
{
    public class CrawlSettingController : Controller
    {
        private readonly ProjectContext _context;
        private readonly JobFilterService _jobFilterService;
        private readonly CrawlService _crawlService;
        private readonly ILogger<CrawlSettingController> _logger;

        public CrawlSettingController(ProjectContext context, ILogger<CrawlSettingController> logger, CrawlService crawlService, JobFilterService jobFilterService)
        {
            _context = context;
            _logger = logger;
            _crawlService = crawlService;
            _jobFilterService = jobFilterService;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var data = await _context.CrawlSetting.ToListAsync();
                return View(data);
            }
            catch (Exception ex)
            {
                _logger.LogError($"{ex.Message}");
                return Content("<h2>Service unavailable.</h2>", "text/html", Encoding.UTF8);
            }
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CrawlSetting data, string seniority1, string seniority2, string seniority3, string seniority4)
        {
            try
            {
                // 調整年資欄位
                List<string> sList = new List<string>();
                if (!string.IsNullOrEmpty(seniority1)) sList.Add(seniority1);
                if (!string.IsNullOrEmpty(seniority2)) sList.Add(seniority2);
                if (!string.IsNullOrEmpty(seniority3)) sList.Add(seniority3);
                if (!string.IsNullOrEmpty(seniority4)) sList.Add(seniority4);
                data.Seniority = string.Join(",", sList);

                // 新增爬蟲設定 & 寫入DB
                _context.Add(data);
                await _context.SaveChangesAsync();
                TempData["message"] = "新增成功";
            }
            catch (Exception ex)
            {
                _logger.LogError($"{ex.Message}\n{ex.StackTrace}");
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

                var data = await _context.CrawlSetting.FirstOrDefaultAsync(u => u.Id == id);

                if (data == null)
                {
                    return NotFound();
                }

                return View(data);
            }
            catch (Exception ex)
            {
                _logger.LogError($"{ex.Message}\n{ex.StackTrace}");
                TempData["message"] = "操作失敗";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CrawlSetting data, string seniority1, string seniority2, string seniority3, string seniority4)
        {
            try
            {
                // 調整年資欄位
                List<string> sList = new List<string>();
                if (!string.IsNullOrEmpty(seniority1)) sList.Add(seniority1);
                if (!string.IsNullOrEmpty(seniority2)) sList.Add(seniority2);
                if (!string.IsNullOrEmpty(seniority3)) sList.Add(seniority3);
                if (!string.IsNullOrEmpty(seniority4)) sList.Add(seniority4);
                data.Seniority = string.Join(",", sList);

                // 設定欲修改的欄位
                _context.Entry(data).Property(p => p.Remark).IsModified = true;
                _context.Entry(data).Property(p => p.TargetUrl).IsModified = true;
                _context.Entry(data).Property(p => p.MinSalary).IsModified = true;
                _context.Entry(data).Property(p => p.MaxSalary).IsModified = true;
                _context.Entry(data).Property(p => p.Seniority).IsModified = true;
                _context.Entry(data).Property(p => p.ExcludeWords).IsModified = true;
                _context.Entry(data).Property(p => p.IncludeWords).IsModified = true;
                _context.Entry(data).Property(p => p.HasSalary).IsModified = true;
                await _context.SaveChangesAsync();
                TempData["message"] = "修改成功";
            }
            catch (Exception ex)
            {
                _logger.LogError($"{ex.Message}\n{ex.StackTrace}");
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
                return "刪除成功";
            }
            catch (Exception ex)
            {
                _logger.LogError($"{ex.Message}\n{ex.StackTrace}");
                return "操作失敗";
            }
        }

        public async Task<IActionResult> DoCrawl(int? id)
        {
            try
            {
                // 爬取目標頁面，提取工作列表
                var crawlSetting = await _context.CrawlSetting.FirstOrDefaultAsync(u => u.Id == id);
                List<JobItem> jobItems = await _crawlService.GetTargetItems(crawlSetting);

                // 若職缺數量為零，視為搜尋失敗
                if (jobItems.Count == 0)
                {
                    TempData["message"] = "搜尋失敗";
                    return RedirectToAction("Index");
                }

                // 過濾掉已封鎖的工作和公司
                jobItems = await _jobFilterService.GetUnblockedItems(jobItems);

                // 過濾掉職稱裡面含有特定關鍵字的職缺
                jobItems = _jobFilterService.FilterByExcludeWords(jobItems, crawlSetting.ExcludeWords);

                // 過濾掉職稱裡面沒有特定關鍵字的職缺
                jobItems = _jobFilterService.FilterByIncludeWords(jobItems, crawlSetting.IncludeWords);

                // 過濾掉最高月薪開太低的職缺(注意，這裡的設計會將待遇面議的職缺過濾掉)
                jobItems = _jobFilterService.FilterByMaxSalary(jobItems, crawlSetting.MaxSalary);

                // 過濾掉外派駐點的職缺
                jobItems = _jobFilterService.FilterExpatriate(jobItems);

                // 將最終結果儲存到 Session
                HttpContext.Session.SetString("jobItems", JsonConvert.SerializeObject(jobItems));
                return RedirectToAction("JobItems");
            }
            catch (Exception ex)
            {
                _logger.LogError($"{ex.Message}\n{ex.StackTrace}");
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
