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
    public class BlockJobItemController : Controller
    {
        private readonly JobFilterContext _context;
        private readonly CrawlService crawlService = new CrawlService();

        public BlockJobItemController(JobFilterContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.BlockJobItems.ToListAsync());
        }

        public IActionResult Create(string JobCode = null)
        {
            ViewBag.JobCode = JobCode;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(IFormCollection PostData)
        {
            // 取出各欄位的值
            string jobCode = PostData["jobCode"].ToString() ?? null;
            string blockReason = PostData["blockReason"].ToString() ?? null;

            // 新增封鎖工作
            BlockJobItem blockJobItem = new BlockJobItem
            {
                JobCode = jobCode,
                BlockReason = blockReason,
            };

            _context.Add(blockJobItem);
            await _context.SaveChangesAsync();

            // 刷新SESSION儲存的工作項目
            string jobItemsStr = HttpContext.Session.GetString("jobItems");
            if (jobItemsStr != null)
            {
                List<JobItem> jobItems = JsonConvert.DeserializeObject<List<JobItem>>(HttpContext.Session.GetString("jobItems"));
                jobItems = crawlService.GetUnblockedItems(_context, jobItems);
                HttpContext.Session.SetString("jobItems", JsonConvert.SerializeObject(jobItems));
            }

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

            var blockJobItem = await _context.BlockJobItems.FirstOrDefaultAsync(u => u.Id == id);

            if (blockJobItem == null)
            {
                return NotFound();
            }

            #endregion

            return View(blockJobItem);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(IFormCollection PostData)
        {
            // 取出各欄位的值
            int id = int.Parse(PostData["Id"].ToString());
            string jobCode = PostData["jobCode"].ToString() ?? null;
            string blockReason = PostData["blockReason"].ToString() ?? null;

            // 取得該筆資料
            var blockJobItem = await _context.BlockJobItems.FirstOrDefaultAsync(u => u.Id == id);
            if (blockJobItem == null)
            {
                TempData["message"] = "修改失敗，此筆資料已被刪除";
                return RedirectToAction("Edit", new { id });
            }

            // 修改該筆資料
            blockJobItem.JobCode = jobCode;
            blockJobItem.BlockReason = blockReason;
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

            var blockJobItem = _context.BlockJobItems.FirstOrDefault(u => u.Id == id);

            if (blockJobItem == null)
            {
                return "刪除失敗，查無這筆資料!";
            }

            #endregion

            // 刪除該筆資料
            _context.Remove(blockJobItem);
            _context.SaveChanges();
            
            return "刪除成功";
        }
    }
}
