using JobFilter2.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JobFilter2.Controllers
{
    public class BlockJobItemController : Controller
    {
        private readonly JobFilterContext _context;

        public BlockJobItemController(JobFilterContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.BlockJobItems.ToListAsync());
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
            string jobCode = PostData["jobCode"].ToString() ?? null;
            string blockReason = PostData["blockReason"].ToString() ?? null;

            // 新增封鎖工作
            BlockJobItem blockJobItem = new BlockJobItem
            {
                JobCode = jobCode,
                BlockReason = blockReason,
            };

            _context.BlockJobItems.Add(blockJobItem);
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
                TempData["message"] = "修改失敗，此用戶已被刪除";
                return RedirectToAction("Update", new { id });
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

            // 刪除用戶並寫入DB
            _context.BlockJobItems.Remove(blockJobItem);
            _context.SaveChanges();
            return "刪除成功";
        }
    }
}
