using JobFilter2.Models;
using JobFilter2.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

        public BlockJobItemController(JobFilterContext context)
        {
            _context = context;
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
                List<JobItem> jobItems = JsonConvert.DeserializeObject<List<JobItem>>(HttpContext.Session.GetString("jobItems"));
                jobItems = crawlService.GetUnblockedItems(_context, jobItems);
                HttpContext.Session.SetString("jobItems", JsonConvert.SerializeObject(jobItems));
            }

            return "封鎖成功";
        }
    }
}
