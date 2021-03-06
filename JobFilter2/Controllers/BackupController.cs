using JobFilter2.Models;
using JobFilter2.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.IO;

namespace JobFilter2.Controllers
{
    public class BackupController : Controller
    {
        private readonly JobFilterContext _context;
        private readonly ILogger<BackupController> _logger;
        private readonly BackupService backupService = new BackupService();

        public BackupController(JobFilterContext context, ILogger<BackupController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpPost]
        public string Export(IFormCollection PostData)
        {
            try
            {
                // 提取資料夾路徑
                string exportPath = PostData["exportPath"].ToString();

                // 檢查路徑是否存在
                if (!Directory.Exists(exportPath))
                {
                    return "路徑錯誤";
                }

                // 將DB資料匯出到目標資料夾
                backupService.Export(_context, exportPath);
                return "匯出成功";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return "操作失敗";
            }
        }

        [HttpPost]
        public string Import(IFormCollection PostData)
        {
            try
            {
                // 提取資料夾路徑
                string importPath = PostData["importPath"].ToString();

                // 檢查路徑是否存在
                if (!Directory.Exists(importPath))
                {
                    return "路徑錯誤";
                }

                // 從目標資料夾讀取CSV並匯入DB
                backupService.Import(_context, importPath);
                return "匯入成功";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return "操作失敗";
            }
        }
    }
}
