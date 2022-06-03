using CsvHelper;
using CsvHelper.Configuration;
using JobFilter2.Models;
using JobFilter2.Models.Entities;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace JobFilter2.Services
{
    public class BackupService
    {
        #region 令匯入的時候忽略Id屬性

        // 讀取的時候忽略ID屬性
        private class CrawlSettingMap : ClassMap<CrawlSetting>
        {
            public CrawlSettingMap()
            {
                AutoMap(CultureInfo.InvariantCulture);
                Map(m => m.Id).Ignore();
            }
        }

        // 讀取的時候忽略ID屬性
        private class BlockCompanyMap : ClassMap<BlockCompany>
        {
            public BlockCompanyMap()
            {
                AutoMap(CultureInfo.InvariantCulture);
                Map(m => m.Id).Ignore();
            }
        }
        // 讀取的時候忽略ID屬性
        private class BlockJobItemMap : ClassMap<BlockJobItem>
        {
            public BlockJobItemMap()
            {
                AutoMap(CultureInfo.InvariantCulture);
                Map(m => m.Id).Ignore();
            }
        }

        #endregion

        public void Export(JobFilterContext _context, string exportPath)
        {
            try
            {
                // 切換路徑到目標資料夾
                Directory.SetCurrentDirectory(exportPath);

                // 撈出欲備份的資料
                List<CrawlSetting> DataList1 = _context.CrawlSettings.ToList();
                List<BlockJobItem> DataList2 = _context.BlockJobItems.ToList();
                List<BlockCompany> DataList3 = _context.BlockCompanies.ToList();

                // 寫入CSV檔案(爬蟲設定)
                using var writer1 = new StreamWriter("CrawlSettings.csv", false, Encoding.UTF8);
                using var csvWriter1 = new CsvWriter(writer1, CultureInfo.InvariantCulture);
                csvWriter1.WriteRecords(DataList1);

                // 寫入CSV檔案(封鎖工作)
                using var writer2 = new StreamWriter("BlockJobItems.csv", false, Encoding.UTF8);
                using var csvWriter2 = new CsvWriter(writer2, CultureInfo.InvariantCulture);
                csvWriter2.WriteRecords(DataList2);

                // 寫入CSV檔案(封鎖公司)
                using var writer3 = new StreamWriter("BlockCompanies.csv", false, Encoding.UTF8);
                using var csvWriter3 = new CsvWriter(writer3, CultureInfo.InvariantCulture);
                csvWriter3.WriteRecords(DataList3);
            }
            catch(Exception)
            {
            }
        }

        public void Import(JobFilterContext _context, string importPath)
        {
            try
            {
                string fPath1 = importPath + "\\CrawlSettings.csv";
                string fPath2 = importPath + "\\BlockJobItems.csv";
                string fPath3 = importPath + "\\BlockCompanies.csv";

                // 讀取爬蟲設定
                using var reader1 = new StreamReader(fPath1, Encoding.UTF8);
                var csvReader1 = new CsvReader(reader1, CultureInfo.InvariantCulture);
                csvReader1.Context.RegisterClassMap<CrawlSettingMap>();
                var DataList1 = csvReader1.GetRecords<CrawlSetting>().ToList();

                // 讀取封鎖工作
                using var reader2 = new StreamReader(fPath2, Encoding.UTF8);
                var csvReader2 = new CsvReader(reader2, CultureInfo.InvariantCulture);
                csvReader2.Context.RegisterClassMap<BlockJobItemMap>();
                var DataList2 = csvReader2.GetRecords<BlockJobItem>().ToList();

                // 讀取封鎖公司
                using var reader3 = new StreamReader(fPath3, Encoding.UTF8);
                var csvReader3 = new CsvReader(reader3, CultureInfo.InvariantCulture);
                csvReader3.Context.RegisterClassMap<BlockCompanyMap>();
                var DataList3 = csvReader3.GetRecords<BlockCompany>().ToList();

                // 整理資料內容，等等會用 SQL 做多筆的資料匯入
                List<string> insert_CrawlSetting = new List<string>();
                List<string> insert_BlockJobItem = new List<string>();
                List<string> insert_BlockCompany = new List<string>();
                foreach (var data in DataList1)
                {
                    // 對可能會出現單、雙引號的欄位做字元過濾
                    string targetUrl = data.TargetUrl.Replace("\'", "").Replace("\"","");
                    string seniority = data.Seniority.Replace("\'", "").Replace("\"", "");
                    string remark = data.Remark.Replace("\'", "").Replace("\"", "");
                    insert_CrawlSetting.Add($"(\'{targetUrl}\',\'{data.MinSalary}\',\'{seniority}\',\'{remark}\')");
                }

                foreach (var data in DataList2)
                {
                    insert_BlockJobItem.Add($"(\'{data.JobCode}\')");
                }

                foreach (var data in DataList3)
                {
                    // 對可能會出現單、雙引號的欄位做字元過濾
                    string company_name = data.CompanyName.Replace("\'", "").Replace("\"", "");
                    string block_reason = data.BlockReason.Replace("\'", "").Replace("\"", "");
                    insert_BlockCompany.Add($"(\'{company_name}\',\'{block_reason}\')");
                }

                // 多筆資料匯入
                _context.Database.ExecuteSqlRaw($"INSERT INTO CrawlSetting VALUES {string.Join(",", insert_CrawlSetting)}");
                _context.Database.ExecuteSqlRaw($"INSERT INTO BlockJobItem VALUES {string.Join(",", insert_BlockJobItem)}");
                _context.Database.ExecuteSqlRaw($"INSERT INTO BlockCompany VALUES {string.Join(",", insert_BlockCompany)}");
            }
            catch (Exception)
            {
            }
        }
    }
}
