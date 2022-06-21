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

        public BackupController(JobFilterContext context)
        {
            _context = context;
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
                TempData["message"] = "操作失敗";
                return View();
            }

            // 讀取DB並將資料匯出到目標資料夾
            bool isExportSuccess = backupService.Export(_context, exportPath);
            TempData["message"] = isExportSuccess ? "匯出成功" : "操作失敗";
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
                TempData["message"] = "操作失敗";
                return View();
            }

            // 讀取CSV檔案並將資料匯入到DB
            bool isImportSuccess = backupService.Import(_context, importPath);
            TempData["message"] = isImportSuccess ? "匯入成功" : "操作失敗";
            return RedirectToRoute(new { controller = "Home", action = "Index" });
        }
    }
}
