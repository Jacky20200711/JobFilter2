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
    public class BlockCompanyController : Controller
    {
        private readonly JobFilterContext _context;
        private readonly CrawlService crawlService = new CrawlService();

        public BlockCompanyController(JobFilterContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.BlockCompanies.ToListAsync());
        }

        public IActionResult Create(string Company = null)
        {
            ViewBag.Company = Company;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(IFormCollection PostData)
        {
            // 取出各欄位的值
            string companyName = PostData["companyName"].ToString() ?? null;
            string blockReason = PostData["blockReason"].ToString() ?? null;

            // 新增封鎖工作
            BlockCompany blockCompany = new BlockCompany
            {
                CompanyName = companyName,
                BlockReason = blockReason,
            };

            _context.Add(blockCompany);
            await _context.SaveChangesAsync();
            TempData["message"] = "新增成功";

            // 刷新SESSION儲存的工作項目
            string jobItemsStr = HttpContext.Session.GetString("jobItems");
            if (jobItemsStr != null)
            {
                List<JobItem> jobItems = JsonConvert.DeserializeObject<List<JobItem>>(HttpContext.Session.GetString("jobItems"));
                jobItems = crawlService.GetUnblockedItems(_context, jobItems);
                HttpContext.Session.SetString("jobItems", JsonConvert.SerializeObject(jobItems));
                return RedirectToRoute(new { controller = "CrawlSetting", action = "JobItems" }); // 返回過濾結果
            }

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Edit(int? id)
        {
            #region 檢查此筆資料是否存在，若不存在則跳轉到錯誤頁面

            if (id == null)
            {
                return NotFound();
            }

            var blockCompany = await _context.BlockCompanies.FirstOrDefaultAsync(u => u.Id == id);

            if (blockCompany == null)
            {
                return NotFound();
            }

            #endregion

            return View(blockCompany);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(IFormCollection PostData)
        {
            // 取出各欄位的值
            int id = int.Parse(PostData["Id"].ToString());
            string companyName = PostData["companyName"].ToString() ?? null;
            string blockReason = PostData["blockReason"].ToString() ?? null;

            // 取得該筆資料
            var blockCompany = await _context.BlockCompanies.FirstOrDefaultAsync(u => u.Id == id);
            if (blockCompany == null)
            {
                return NotFound();
            }

            // 修改該筆資料
            blockCompany.CompanyName = companyName;
            blockCompany.BlockReason = blockReason;
            await _context.SaveChangesAsync();

            TempData["message"] = "修改成功";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<string> Delete(int? id)
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

            // 更新DB
            _context.Remove(blockCompany);
            await _context.SaveChangesAsync();

            return "刪除成功";
        }
    }
}
