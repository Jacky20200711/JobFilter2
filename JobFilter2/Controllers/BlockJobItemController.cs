using JobFilter2.Models.Entities;
using JobFilter2.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace JobFilter2.Controllers
{
    public class BlockJobItemController : Controller
    {
        private readonly JobFilterContext _context;
        private readonly CrawlService _crawlService;
        private readonly ILogger<BlockJobItemController> _logger;

        public BlockJobItemController(JobFilterContext context, ILogger<BlockJobItemController> logger, CrawlService crawlService)
        {
            _context = context;
            _logger = logger;
            _crawlService = crawlService;
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
                _crawlService.UpdateJobList(data.JobCode, blockType: "jobCode");
               
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
