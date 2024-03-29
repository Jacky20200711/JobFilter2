﻿using JobFilter2.Models.Entities;
using JobFilter2.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace JobFilter2.Controllers
{
    public class BlockJobItemController : Controller
    {
        private readonly ProjectContext _context;
        private readonly JobFilterService _jobFilterService;
        private readonly ILogger<BlockJobItemController> _logger;

        public BlockJobItemController(ProjectContext context, ILogger<BlockJobItemController> logger, JobFilterService jobFilterService)
        {
            _context = context;
            _logger = logger;
            _jobFilterService = jobFilterService;
        }

        [HttpPost]
        public async Task<string> Create(BlockJobItem data)
        {
            try
            {
                // 新增封鎖工作 & 寫入DB
                _context.Add(data);
                await _context.SaveChangesAsync();

                // 刷新 Session 儲存的工作列表
                _jobFilterService.UpdateJobList(data.JobCode, blockType: "jobCode");
               
                return "封鎖成功";
            }
            catch(Exception ex)
            {
                _logger.LogError($"{ex.Message}\n{ex.StackTrace}");
                return "操作失敗";
            }
        }
    }
}
