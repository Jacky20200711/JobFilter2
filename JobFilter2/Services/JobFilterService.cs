using JobFilter2.Models.Entities;
using Microsoft.AspNetCore.Http;
using AngleSharp.Text;
using JobFilter2.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JobFilter2.Services
{
    public class JobFilterService
    {
        private readonly JobFilterContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public JobFilterService(JobFilterContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// 根據DB的封鎖工作與封鎖公司，來過濾傳入的工作列表
        /// </summary>
        /// <returns>過濾後的工作列表</returns>
        public async Task<List<JobItem>> GetUnblockedItems(List<JobItem> jobItems)
        {
            List<JobItem> new_jobitems = new List<JobItem>();

            // 取得已封鎖的工作代碼與公司名稱
            var blockJobItems = await _context.BlockJobItems.ToListAsync();
            var blockCompanys = await _context.BlockCompanies.ToListAsync();

            // 將已封鎖的工作代碼和公司名稱，轉存到 HashTable 以加速搜尋比對
            HashSet<string> blockJobCodeSet = new HashSet<string>();
            HashSet<string> blockCompanySet = new HashSet<string>();
            blockJobItems.ForEach(x => blockJobCodeSet.Add(x.JobCode));
            blockCompanys.ForEach(x => blockCompanySet.Add(x.CompanyName));

            // 檢查傳入的工作列表，取出沒有被過濾的工作
            foreach (var jobItem in jobItems)
            {
                if (!blockJobCodeSet.Contains(jobItem.Code) && !blockCompanySet.Contains(jobItem.Company))
                {
                    new_jobitems.Add(jobItem);
                }
            }

            return new_jobitems;
        }

        /// <summary>
        /// 刷新 Session 儲存的工作列表(當封鎖工作或封鎖公司之後，都會Call這個函數)
        /// </summary>
        public void UpdateJobList(string target, string blockType)
        {
            List<JobItem> new_jobitems = new List<JobItem>();

            // 從 Session 取出工作列表
            string jobItemsStr = _httpContextAccessor.HttpContext.Session.GetString("jobItems");
            if (jobItemsStr == null)
            {
                return;
            }

            List<JobItem> jobItems = JsonConvert.DeserializeObject<List<JobItem>>(jobItemsStr);

            // 判斷 User 是要封鎖工作還是封鎖公司
            if (blockType == "jobCode")
            {
                // 觀察工作列表，取出未被封鎖的工作項目
                foreach (var jobItem in jobItems)
                {
                    if (jobItem.Code != target)
                    {
                        new_jobitems.Add(jobItem);
                    }
                }
            }
            else if (blockType == "company")
            {
                // 觀察工作列表，取出未被封鎖的工作項目
                foreach (var jobItem in jobItems)
                {
                    if (jobItem.Company != target)
                    {
                        new_jobitems.Add(jobItem);
                    }
                }
            }

            // 刷新 Session 儲存的工作列表
            _httpContextAccessor.HttpContext.Session.SetString("jobItems", JsonConvert.SerializeObject(new_jobitems));
        }

        /// <summary>
        /// 過濾掉職稱裡面含有特定關鍵字的職缺
        /// </summary>
        /// <returns>過濾後的工作列表</returns>
        public List<JobItem> FilterByExcludeWords(List<JobItem> jobItems, string excludeWords)
        {
            if (string.IsNullOrEmpty(excludeWords))
            {
                return jobItems;
            }

            List<JobItem> new_jobItems = new List<JobItem>();
            List<string> exWords = excludeWords.Split(',').ToList();

            foreach (var job in jobItems)
            {
                // 檢查職稱是否包含關鍵字
                bool hasWord = false;
                foreach (string word in exWords)
                {
                    // 考慮到英文單字，兩邊都轉成小寫再進行比對
                    // 添加 Trim 可以忽略多餘的空白
                    if (job.Title.ToLower().Contains(word.Trim().ToLower()))
                    {
                        hasWord = true;
                        break;
                    }
                }

                if (!hasWord)
                {
                    new_jobItems.Add(job);
                }
            }
            return new_jobItems;
        }

        /// <summary>
        /// 過濾掉最高月薪開太低的職缺
        /// </summary>
        /// <returns>過濾後的工作列表</returns>
        public List<JobItem> FilterByMaxSalary(List<JobItem> jobItems, int maxSalaryOfSetting)
        {
            List<JobItem> new_jobs = new List<JobItem>();

            foreach (var job in jobItems)
            {
                // 若是待遇面議則直接添加
                if(job.Salary == "待遇面議")
                {
                    new_jobs.Add(job);
                    continue;
                }

                // 預設從頭搜尋數字
                string searchContent = job.Salary;

                // 如果薪水為一個範圍，則從'~'右方搜尋數字
                bool hasSalaryRange = false;
                if (job.Salary.Contains('~'))
                {
                    searchContent = job.Salary.Split('~')[1];
                    hasSalaryRange = true;
                }

                // 開始搜尋數字並儲存
                List<char> digits = new List<char>();
                foreach (char c in searchContent)
                {
                    if (c.IsDigit())
                    {
                        digits.Add(c);
                    }
                }

                // 將這些字元重新拼接成數字
                string digitToStr = new string(digits.ToArray());
                int checkNum = int.Parse(digitToStr);

                // 如果薪水不是一個範圍，例如"月薪 N 以上"，則判斷 N 是否夠大
                if (!hasSalaryRange)
                {
                    // 若 N 為 60000 以上才添加，否則視為低薪職缺並果斷忽略
                    if (checkNum >= 60000)
                    {
                        new_jobs.Add(job);
                    }
                }
                // 如果薪水屬於一個範圍，則判斷上限是否符合期望
                else
                {
                    if (checkNum >= maxSalaryOfSetting)
                    {
                        new_jobs.Add(job);
                    }
                }
            }

            return new_jobs;
        }

        /// <summary>
        /// 過濾掉派遣駐點的職缺
        /// </summary>
        /// <returns>過濾後的工作列表</returns>
        public List<JobItem> FilterExpatriate(List<JobItem> jobItems)
        {
            List<JobItem> new_jobs = new List<JobItem>();

            foreach (var job in jobItems)
            {
                // 如果地址長度太小，判定為"XX市XX區"的外派職缺，果斷忽略
                // 如果地址尾端為外派職缺特有，果斷忽略
                string lastTwoChars = job.Address.Substring(job.Address.Length - 2);
                if (job.Address.Length <= 6 || lastTwoChars == "市內" || lastTwoChars == "附近" || lastTwoChars == "北市" || lastTwoChars[^1] == '段')
                {
                    continue;
                }
                else
                {
                    new_jobs.Add(job);
                }
            }

            return new_jobs;
        }
    }
}
