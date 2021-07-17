﻿using JobFilter2.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JobFilter2.Controllers
{
    public class BlockCompanyController : Controller
    {
        private readonly JobFilterContext _context;

        public BlockCompanyController(JobFilterContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.BlockCompanies.ToListAsync());
        }

        public IActionResult Create()
        {
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

            _context.BlockCompanies.Add(blockCompany);
            await _context.SaveChangesAsync();

            TempData["message"] = "新增成功";
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
                TempData["message"] = "修改失敗，此筆資料已被刪除";
                return RedirectToAction("Edit", new { id });
            }

            // 修改該筆資料
            blockCompany.CompanyName = companyName;
            blockCompany.BlockReason = blockReason;
            await _context.SaveChangesAsync();

            TempData["message"] = "修改成功";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public string Delete(int? id)
        {
            #region 檢查此筆資料是否存在

            if (id == null)
            {
                return "刪除失敗，查無這筆資料!";
            }

            var blockCompany = _context.BlockCompanies.FirstOrDefault(u => u.Id == id);

            if (blockCompany == null)
            {
                return "刪除失敗，查無這筆資料!";
            }

            #endregion

            // 刪除用戶並寫入DB
            _context.BlockCompanies.Remove(blockCompany);
            _context.SaveChanges();
            return "刪除成功";
        }
    }
}
