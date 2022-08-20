using JobFilter2.Models;
using JobFilter2.Models.Entities;
using JobFilter2.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JobFilter2.Controllers
{
    public class BlockJobItemController : Controller
    {
        private readonly JobFilterContext _context;
        private readonly CrawlService crawlService = new CrawlService();
        private readonly ILogger<BlockJobItemController> _logger;

        public BlockJobItemController(JobFilterContext context, ILogger<BlockJobItemController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpPost]
        public async Task<string> Create(BlockJobItem data)
        {
            try
            {
                // 新增封鎖工作 & 寫入DB
                _context.Add(data);
                await _context.SaveChangesAsync();

                // 刷新 Session 儲存的工作項目
                string jobItemsStr = HttpContext.Session.GetString("jobItems");
                if (jobItemsStr != null)
                {
                    List<JobItem> jobItems = JsonConvert.DeserializeObject<List<JobItem>>(jobItemsStr);
                    jobItems = crawlService.GetUpdateList(jobItems, data.JobCode, blockType: "jobCode");
                    HttpContext.Session.SetString("jobItems", JsonConvert.SerializeObject(jobItems));
                }

                return "封鎖成功";
            }
            catch(Exception ex)
            {
                _logger.LogError(ex.ToString());
                return "操作失敗";
            }
        }
    }
}
