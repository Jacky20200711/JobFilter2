using JobFilter2.Models.Entities;
using JobFilter2.Models;
using JobFilter2.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.IO;

namespace JobFilter2.Controllers
{
    public class BackupController : Controller
    {
        private readonly ProjectContext _context;
        private readonly ILogger<BackupController> _logger;
        private readonly BackupService _backupService;

        public BackupController(ProjectContext context, ILogger<BackupController> logger, BackupService backupService)
        {
            _context = context;
            _logger = logger;
            _backupService = backupService;
        }

        [HttpPost]
        public Result Export(string exportPath)
        {
            Result result = new Result
            {
                Code= 0,
                Message = "操作失敗"
            };

            try
            {
                // 檢查目標路徑是否存在
                if (!Directory.Exists(exportPath))
                {
                    result.Message = "路徑錯誤";
                    return result;
                }

                // 匯出到目標資料夾
                _backupService.Export(exportPath);
                result.Message = "匯出成功";
                result.Code = 1;
            }
            catch (Exception ex)
            {
                _logger.LogError($"{ex.Message}\n{ex.StackTrace}");
            }
            return result;
        }

        [HttpPost]
        public Result Import(string importPath)
        {
            Result result = new Result
            {
                Code = 0,
                Message = "操作失敗"
            };

            try
            {
                // 檢查目標路徑是否存在
                if (!Directory.Exists(importPath))
                {
                    result.Message = "路徑錯誤";
                    return result;
                }

                // 匯入前先清空各資料表
                _context.Database.ExecuteSqlRaw($"DELETE FROM CrawlSetting");
                _context.Database.ExecuteSqlRaw($"DELETE FROM BlockJobItem");
                _context.Database.ExecuteSqlRaw($"DELETE FROM BlockCompany");

                // 從目標資料夾讀取CSV並匯入DB
                _backupService.Import(importPath);
                result.Message = "匯入成功";
                result.Code = 1;
                TempData["message"] = result.Message;
            }
            catch (Exception ex)
            {
                _logger.LogError($"{ex.Message}\n{ex.StackTrace}");
            }
            return result;
        }
    }
}
