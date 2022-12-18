﻿using JobFilter2.Models.Entities;
using JobFilter2.Models;
using JobFilter2.Services;
using Microsoft.AspNetCore.Http;
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
        private readonly BackupService backupService = new BackupService();

        public BackupController(JobFilterContext context, ILogger<BackupController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpPost]
        public Result Export(IFormCollection PostData)
        {
            Result result = new Result
            {
                Code= 0,
                Message = "操作失敗"
            };

            try
            {
                // 提取資料夾路徑
                string exportPath = PostData["exportPath"].ToString();

                // 檢查路徑是否存在
                if (!Directory.Exists(exportPath))
                {
                    result.Message = "路徑錯誤";
                    return result;
                }

                // 匯出到目標資料夾
                backupService.Export(_context, exportPath);
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
        public Result Import(IFormCollection PostData)
        {
            Result result = new Result
            {
                Code = 0,
                Message = "操作失敗"
            };

            try
            {
                // 匯入前先清空各資料表
                _context.Database.ExecuteSqlRaw($"DELETE FROM CrawlSetting");
                _context.Database.ExecuteSqlRaw($"DELETE FROM BlockJobItem");
                _context.Database.ExecuteSqlRaw($"DELETE FROM BlockCompany");

                // 提取資料夾路徑
                string importPath = PostData["importPath"].ToString();

                // 檢查路徑是否存在
                if (!Directory.Exists(importPath))
                {
                    result.Message = "路徑錯誤";
                    return result;
                }

                // 從目標資料夾讀取CSV並匯入DB
                backupService.Import(_context, importPath);
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
