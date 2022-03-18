using JobFilter2.Models;
using JobFilter2.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.IO;

namespace JobFilter2.Controllers
{
    public class BackupController : Controller
    {
        private readonly JobFilterContext _context;
        private readonly BackupService backupService = new BackupService();
        private readonly ILogger<BackupController> _logger;

        public BackupController(JobFilterContext context, ILogger<BackupController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public IActionResult Export()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Export(IFormCollection PostData)
        {
            // 取出前端送過來的資料夾路徑
            string exportPath = PostData["exportPath"].ToString() ?? null;

            // 檢查此資料夾路徑是否存在
            if (!Directory.Exists(exportPath))
            {
                TempData["message"] = "匯出失敗，此資料夾路徑不存在!";
                return View();
            }

            // 讀取DB並將資料匯出到目標資料夾
            backupService.Export(_context, exportPath);

            // 完成後返回首頁
            TempData["message"] = "匯出成功";
            return RedirectToRoute(new { controller = "Home", action = "Index" });
        }

        public IActionResult Import()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Import(IFormCollection PostData)
        {
            // 取出前端送過來的資料夾路徑
            string importPath = PostData["importPath"].ToString() ?? null;

            // 檢查此資料夾路徑是否存在
            if (!Directory.Exists(importPath))
            {
                TempData["message"] = "匯入失敗，此資料夾路徑不存在!";
                return View();
            }

            // 讀取CSV檔案並將資料匯入到DB
            backupService.Import(_context, importPath);

            // 完成後返回首頁
            TempData["message"] = "匯入成功";
            return RedirectToRoute(new { controller = "Home", action = "Index" });
        }

        /// <summary>
        /// 這個函數主要是用來測試匯入功能(為了方便觀察，所以匯入DB前先刪除DB資料)
        /// </summary>
        public IActionResult ClearBlock()
        {
            using var transaction = _context.Database.BeginTransaction();
            _context.RemoveRange(_context.BlockCompanies);
            _context.RemoveRange(_context.BlockJobItems);
            _context.SaveChanges();
            transaction.Commit();
            TempData["message"] = "封鎖紀錄已清空";
            return RedirectToRoute(new { controller = "Home", action = "Index" });
        }
    }
}
