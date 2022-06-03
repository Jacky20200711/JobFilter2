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
                return Content("<h2>資料庫異常，請聯絡相關人員!</h2>", "text/html", Encoding.UTF8);
            }
        }

        public IActionResult Create(string Company = null)
        {
            ViewBag.Company = Company;
            return View();
        }

        [HttpPost]
        public async Task<string> Create(IFormCollection PostData)
        {
            try
            {
                // 取出各欄位的值
                string company = PostData["companyName"].ToString() ?? null;
                string blockReason = PostData["blockReason"].ToString() ?? null;

                // 檢查長度
                if (company.Length > 100)
                {
                    return "封鎖失敗，公司名稱過長!";
                }

                // 新增封鎖工作
                BlockCompany blockCompany = new BlockCompany
                {
                    CompanyName = company,
                    BlockReason = blockReason,
                };

                _context.Add(blockCompany);
                await _context.SaveChangesAsync();

                // 刷新SESSION儲存的工作項目
                string jobItemsStr = HttpContext.Session.GetString("jobItems");
                if (jobItemsStr != null)
                {
                    List<JobItem> jobItems = JsonConvert.DeserializeObject<List<JobItem>>(jobItemsStr);
                    jobItems = crawlService.GetUpdateList(jobItems, company, blockType: "company");
                    HttpContext.Session.SetString("jobItems", JsonConvert.SerializeObject(jobItems));
                }

                return "封鎖成功";
            }
            catch(Exception ex)
            {
                _logger.LogError(ex.ToString());
                return "封鎖失敗";
            }
        }

        [HttpPost]
        public async Task<string> Edit(int? id, string new_reason)
        {
            try
            {
                #region 檢查此筆資料是否存在

                if (id == null)
                {
                    return "id異常!";
                }

                var data = await _context.BlockCompanies.FirstOrDefaultAsync(u => u.Id == id);

                if (data == null)
                {
                    return "資料不存在!";
                }

                #endregion

                // 檢查長度
                if (new_reason.Length > 20)
                {
                    return "修改失敗，封鎖理由的長度異常!";
                }

                // 修改該筆資料
                data.BlockReason = new_reason;
                await _context.SaveChangesAsync();
                return "修改成功";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return "資料庫異常，請聯絡相關人員!";
            }
        }

        [HttpPost]
        public async Task<string> Delete(int? id)
        {
            try
            {
                #region 檢查此筆資料是否存在

                if (id == null)
                {
                    return "刪除失敗，查無這筆資料!";
                }

                var blockCompany = await _context.BlockCompanies.FindAsync(id);

                if (blockCompany == null)
                {
                    return "刪除失敗，查無這筆資料!";
                }

                #endregion
                _context.Remove(blockCompany);
                await _context.SaveChangesAsync();
                return "刪除成功";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return "資料庫異常，請聯絡相關人員!";
            }
        }
    }
}
