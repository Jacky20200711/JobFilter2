using JobFilter2.Models;
using JobFilter2.Models.Entities;
using JobFilter2.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobFilter2.Controllers
{
    public class BlockCompanyController : Controller
    {
        private readonly ProjectContext _context;
        private readonly JobFilterService _jobFilterService;
        private readonly ILogger<BlockCompanyController> _logger;

        public BlockCompanyController(ProjectContext context, ILogger<BlockCompanyController> logger, JobFilterService jobFilterService)
        {
            _context = context;
            _logger = logger;
            _jobFilterService = jobFilterService;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var data = await _context.BlockCompany.OrderByDescending(c => c.Id).ToListAsync();
                return View(data);
            }
            catch (Exception ex)
            {
                _logger.LogError($"{ex.Message}");
                return Content("<h2>Service unavailable.</h2>", "text/html", Encoding.UTF8);
            }
        }

        [HttpPost]
        public async Task<string> Create(BlockCompany data)
        {
            try
            {
                // 新增封鎖公司 & 寫入DB
                _context.Add(data);
                await _context.SaveChangesAsync();

                // 刷新 Session 儲存的工作列表
                _jobFilterService.UpdateJobList(data.CompanyName, blockType: "company");

                return "封鎖成功";
            }
            catch(Exception ex)
            {
                _logger.LogError($"{ex.Message}\n{ex.StackTrace}");
                return "操作失敗";
            }
        }

        [HttpPost]
        public async Task<Result> Edit(BlockCompany data)
        {
            Result result = new Result
            {
                Code = 0,
                Message = "操作失敗"
            };

            try
            {
                _context.Entry(data).Property(p => p.BlockReason).IsModified = true;
                await _context.SaveChangesAsync();
                result.Data = Utility.BLOCK_REASON[data.BlockReason];
                result.Message = "修改成功";
                result.Code = 1;
            }
            catch (Exception ex)
            {
                _logger.LogError($"{ex.Message}\n{ex.StackTrace}");
            }
            return result;
        }

        [HttpPost]
        public async Task<string> Delete(BlockCompany data)
        {
            try
            {
                _context.Entry(data).State = EntityState.Deleted;
                await _context.SaveChangesAsync();
                return "刪除成功";
            }
            catch (Exception ex)
            {
                _logger.LogError($"{ex.Message}\n{ex.StackTrace}");
                return "操作失敗";
            }
        }
    }
}
