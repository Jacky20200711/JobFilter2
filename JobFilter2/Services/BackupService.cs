using CsvHelper;
using CsvHelper.Configuration;
using JobFilter2.Models;
using JobFilter2.Models.Entities;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using NLog;
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
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

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
            catch(Exception ex)
            {
                throw ex;
            }
        }

        public void Import(JobFilterContext _context, string importPath)
        {
            try
            {
                string fPath1 = importPath + "\\CrawlSettings.csv";
                string fPath2 = importPath + "\\BlockJobItems.csv";
                string fPath3 = importPath + "\\BlockCompanies.csv";

                // 這些變數用來儲存從檔案讀取出來的資料
                List<CrawlSetting> DataList1 = new List<CrawlSetting>();
                List<BlockJobItem> DataList2 = new List<BlockJobItem>();
                List<BlockCompany> DataList3 = new List<BlockCompany>();

                // 這些變數用來整理資料內容，等等會用 SQL 做多筆的資料匯入
                List<string> insert_CrawlSetting = new List<string>();
                List<string> insert_BlockJobItem = new List<string>();
                List<string> insert_BlockCompany = new List<string>();
                List<SqlParameter> sqlParameter_CrawlSetting = new List<SqlParameter>();
                List<SqlParameter> sqlParameter_BlockJobItem = new List<SqlParameter>();
                List<SqlParameter> sqlParameter_BlockCompany = new List<SqlParameter>();
                int no = 0;

                // 若備份檔案存在，才會進一步讀取資料
                if (File.Exists(fPath1))
                {
                    // 讀取檔案資料並轉成LIST
                    using var reader1 = new StreamReader(fPath1, Encoding.UTF8);
                    var csvReader1 = new CsvReader(reader1, CultureInfo.InvariantCulture);
                    csvReader1.Context.RegisterClassMap<CrawlSettingMap>();
                    DataList1 = csvReader1.GetRecords<CrawlSetting>().ToList();

                    // 若LIST不為空，才會進一步設定並執行SQL
                    if(DataList1.Count > 0)
                    {
                        // 設定等等要 INSERT 的 SQL 指令
                        foreach (var data in DataList1)
                        {
                            insert_CrawlSetting.Add($"(@TargetUrl{no},@MinSalary{no},@Seniority{no},@Remark{no})");
                            sqlParameter_CrawlSetting.Add(new SqlParameter($"@TargetUrl{no}", data.TargetUrl));
                            sqlParameter_CrawlSetting.Add(new SqlParameter($"@MinSalary{no}", data.MinSalary));
                            sqlParameter_CrawlSetting.Add(new SqlParameter($"@Seniority{no}", data.Seniority));
                            sqlParameter_CrawlSetting.Add(new SqlParameter($"@Remark{no}", data.Remark));
                            no++;
                        }

                        // 執行 SQL 指令
                        _context.Database.ExecuteSqlRaw($"INSERT INTO CrawlSetting VALUES {string.Join(",", insert_CrawlSetting)}", sqlParameter_CrawlSetting);
                    }
                }

                // 若備份檔案存在，才會進一步讀取資料
                if (File.Exists(fPath2))
                {
                    // 讀取檔案資料並轉成LIST
                    using var reader2 = new StreamReader(fPath2, Encoding.UTF8);
                    var csvReader2 = new CsvReader(reader2, CultureInfo.InvariantCulture);
                    csvReader2.Context.RegisterClassMap<BlockJobItemMap>();
                    DataList2 = csvReader2.GetRecords<BlockJobItem>().ToList();

                    // 若LIST不為空，才會進一步設定並執行SQL
                    if (DataList2.Count > 0)
                    {
                        // 設定等等要 INSERT 的 SQL 指令
                        foreach (var data in DataList2)
                        {
                            insert_BlockJobItem.Add($"(@JobCode{no})");
                            sqlParameter_BlockJobItem.Add(new SqlParameter($"@JobCode{no}", data.JobCode));
                            no++;
                        }

                        // 執行 SQL 指令
                        _context.Database.ExecuteSqlRaw($"INSERT INTO BlockJobItem VALUES {string.Join(",", insert_BlockJobItem)}", sqlParameter_BlockJobItem);
                    }
                }

                // 若備份檔案存在，才會進一步讀取資料
                if (File.Exists(fPath3))
                {
                    // 讀取檔案資料並轉成LIST
                    using var reader3 = new StreamReader(fPath3, Encoding.UTF8);
                    var csvReader3 = new CsvReader(reader3, CultureInfo.InvariantCulture);
                    csvReader3.Context.RegisterClassMap<BlockCompanyMap>();
                    DataList3 = csvReader3.GetRecords<BlockCompany>().ToList();

                    // 若LIST不為空，才會進一步設定並執行SQL
                    if (DataList3.Count > 0)
                    {
                        // 設定等等要 INSERT 的 SQL 指令
                        foreach (var data in DataList3)
                        {
                            insert_BlockCompany.Add($"(@CompanyName{no},@BlockReason{no})");
                            sqlParameter_BlockCompany.Add(new SqlParameter($"@CompanyName{no}", data.CompanyName));
                            sqlParameter_BlockCompany.Add(new SqlParameter($"@BlockReason{no}", data.BlockReason));
                            no++;
                        }

                        // 執行 SQL 指令
                        _context.Database.ExecuteSqlRaw($"INSERT INTO BlockCompany VALUES {string.Join(",", insert_BlockCompany)}", sqlParameter_BlockCompany);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
