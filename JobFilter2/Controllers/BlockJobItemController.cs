using JobFilter2.Models;
using JobFilter2.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
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
        public async Task<string> Create(IFormCollection PostData)
        {
            // 取出參數
            string jobCode = PostData["jobCode"].ToString() ?? null;

            // 新增封鎖工作
            BlockJobItem blockJobItem = new BlockJobItem
            {
                JobCode = jobCode,
            };

            _context.Add(blockJobItem);
            await _context.SaveChangesAsync();

            // 刷新SESSION儲存的工作項目
            string jobItemsStr = HttpContext.Session.GetString("jobItems");
            if (jobItemsStr != null)
            {
                List<JobItem> jobItems = JsonConvert.DeserializeObject<List<JobItem>>(jobItemsStr);
                jobItems = crawlService.GetUnblockedItems(_context, jobItems);
                HttpContext.Session.SetString("jobItems", JsonConvert.SerializeObject(jobItems));
            }

            return "封鎖成功";
        }
    }
}
