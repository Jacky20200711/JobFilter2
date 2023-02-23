﻿using JobFilter2.Models.Entities;
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
        private readonly JobFilterContext _context;
        private readonly ILogger<BackupController> _logger;
        private readonly BackupService backupService;

        public BackupController(JobFilterContext context, ILogger<BackupController> logger)
        {
            _context = context;
            _logger = logger;
            backupService = new BackupService(_context);
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
                // 檢查路徑是否存在
                if (!Directory.Exists(exportPath))
                {
                    result.Message = "路徑錯誤";
                    return result;
                }

                // 匯出到目標資料夾
                backupService.Export(exportPath);
                result.Message = "匯出成功";
                result.Code = 1;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
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
                // 檢查路徑是否存在
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
                backupService.Import(importPath);
                result.Message = "匯入成功";
                result.Code = 1;
                TempData["message"] = result.Message;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
            }
            return result;
        }

        public IActionResult ClearBlock()
        {
            try
            {
                _context.Database.ExecuteSqlRaw($"DELETE FROM BlockCompany");
                _context.Database.ExecuteSqlRaw($"DELETE FROM BlockJobItem");
                TempData["message"] = "清除成功";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                TempData["message"] = "操作失敗";
            }
            return RedirectToRoute(new { controller = "BlockCompany", action = "Index" });
        }
    }
}
