using JobFilter2.Models;
using JobFilter2.Models.Entities;
using JobFilter2.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobFilter2.Controllers
{
    public class BlockCompanyController : Controller
    {
        private readonly JobFilterContext _context;
        private readonly CrawlService crawlService = new CrawlService();
        private readonly ILogger<BlockCompanyController> _logger;

        public BlockCompanyController(JobFilterContext context, ILogger<BlockCompanyController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var data = await _context.BlockCompanies.OrderByDescending(c => c.Id).ToListAsync();
                return View(data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                TempData["message"] = "操作失敗";
                return RedirectToRoute(new { controller = "Home", action = "Index" });
            }
        }

        public IActionResult Create(string Company)
        {
            ViewBag.Company = Company;
            return View();
        }

        [HttpPost]
        public async Task<string> Create(IFormCollection PostData)
        {
            try
            {
                // 新增封鎖公司
                BlockCompany blockCompany = new BlockCompany
                {
                    CompanyName = PostData["companyName"].ToString(),
                    BlockReason = PostData["blockReason"].ToString(),
                };
                _context.Add(blockCompany);
                await _context.SaveChangesAsync();

                // 刷新 Session 儲存的工作項目
                string jobItemsStr = HttpContext.Session.GetString("jobItems");
                if (jobItemsStr != null)
                {
                    List<JobItem> jobItems = JsonConvert.DeserializeObject<List<JobItem>>(jobItemsStr);
                    jobItems = crawlService.GetUpdateList(jobItems, blockCompany.CompanyName, blockType: "company");
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

        [HttpPost]
        public async Task<string> Edit(int? id, string new_reason)
        {
            try
            {
                var data = await _context.BlockCompanies.FirstOrDefaultAsync(u => u.Id == id);
                data.BlockReason = new_reason;
                await _context.SaveChangesAsync();
                return "修改成功";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return "操作失敗";
            }
        }

        [HttpPost]
        public async Task<string> Delete(int id)
        {
            try
            {
                BlockCompany data = new BlockCompany() { Id = id };
                _context.Entry(data).State = EntityState.Deleted;
                await _context.SaveChangesAsync();
                return "刪除成功";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return "操作失敗";
            }
        }
    }
}
