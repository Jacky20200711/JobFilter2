﻿using CsvHelper;
using JobFilter2.Models.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace JobFilter2.Services
{
    public class BackupService
    {
        private readonly ProjectContext _context;

        public BackupService(ProjectContext context)
        {
            _context = context;
        }

        /// <summary>
        /// 將DB資料匯出成CSV檔案
        /// </summary>
        public void Export(string exportPath)
        {
            // 切換路徑到目標資料夾
            Directory.SetCurrentDirectory(exportPath);

            // 撈出欲備份的資料
            List<CrawlSetting> DataList1 = _context.CrawlSetting.ToList();
            List<BlockJobItem> DataList2 = _context.BlockJobItem.ToList();
            List<BlockCompany> DataList3 = _context.BlockCompany.ToList();
            List<BlockForever> DataList4 = _context.BlockForever.ToList();

            // 寫入CSV檔案(爬蟲設定)
            using var writer1 = new StreamWriter("CrawlSetting.csv", false, Encoding.UTF8);
            using var csvWriter1 = new CsvWriter(writer1, CultureInfo.InvariantCulture);
            csvWriter1.WriteRecords(DataList1);

            // 寫入CSV檔案(封鎖工作)
            using var writer2 = new StreamWriter("BlockJobItem.csv", false, Encoding.UTF8);
            using var csvWriter2 = new CsvWriter(writer2, CultureInfo.InvariantCulture);
            csvWriter2.WriteRecords(DataList2);

            // 寫入CSV檔案(封鎖公司)
            using var writer3 = new StreamWriter("BlockCompany.csv", false, Encoding.UTF8);
            using var csvWriter3 = new CsvWriter(writer3, CultureInfo.InvariantCulture);
            csvWriter3.WriteRecords(DataList3);

            // 寫入CSV檔案(永久封鎖)
            using var writer4 = new StreamWriter("BlockForever.csv", false, Encoding.UTF8);
            using var csvWriter4 = new CsvWriter(writer4, CultureInfo.InvariantCulture);
            csvWriter4.WriteRecords(DataList4);
        }

        /// <summary>
        /// 將CSV檔案匯入DB
        /// </summary>
        public void Import(string importPath)
        {
            string fPath1 = importPath + "\\CrawlSetting.csv";
            string fPath2 = importPath + "\\BlockJobItem.csv";
            string fPath3 = importPath + "\\BlockCompany.csv";
            string fPath4 = importPath + "\\BlockForever.csv";

            // 這些變數用來儲存從檔案讀取出來的資料
            List<CrawlSetting> DataList1 = new List<CrawlSetting>();
            List<BlockJobItem> DataList2 = new List<BlockJobItem>();
            List<BlockCompany> DataList3 = new List<BlockCompany>();
            List<BlockForever> DataList4 = new List<BlockForever>();

            // 若備份檔案存在，才會進一步讀取資料
            if (File.Exists(fPath1))
            {
                // 讀取檔案資料並轉成LIST
                using var reader1 = new StreamReader(fPath1, Encoding.UTF8);
                var csvReader1 = new CsvReader(reader1, CultureInfo.InvariantCulture);
                DataList1 = csvReader1.GetRecords<CrawlSetting>().ToList();

                // 若LIST不為空，才會將資料匯入DB
                if (DataList1.Count > 0)
                {
                    using var transaction = _context.Database.BeginTransaction();
                    _context.AddRange(DataList1);
                    _context.Database.ExecuteSqlRaw("SET IDENTITY_INSERT dbo.CrawlSetting ON");
                    _context.SaveChanges();
                    _context.Database.ExecuteSqlRaw("SET IDENTITY_INSERT dbo.CrawlSetting OFF");
                    transaction.Commit();
                }
            }

            // 若備份檔案存在，才會進一步讀取資料
            if (File.Exists(fPath2))
            {
                // 讀取檔案資料並轉成LIST
                using var reader2 = new StreamReader(fPath2, Encoding.UTF8);
                var csvReader2 = new CsvReader(reader2, CultureInfo.InvariantCulture);
                DataList2 = csvReader2.GetRecords<BlockJobItem>().ToList();

                // 若LIST不為空，才會將資料匯入DB
                if (DataList2.Count > 0)
                {
                    using var transaction = _context.Database.BeginTransaction();
                    _context.AddRange(DataList2);
                    _context.Database.ExecuteSqlRaw("SET IDENTITY_INSERT dbo.BlockJobItem ON");
                    _context.SaveChanges();
                    _context.Database.ExecuteSqlRaw("SET IDENTITY_INSERT dbo.BlockJobItem OFF");
                    transaction.Commit();
                }
            }

            // 若備份檔案存在，才會進一步讀取資料
            if (File.Exists(fPath3))
            {
                // 讀取檔案資料並轉成LIST
                using var reader3 = new StreamReader(fPath3, Encoding.UTF8);
                var csvReader3 = new CsvReader(reader3, CultureInfo.InvariantCulture);
                DataList3 = csvReader3.GetRecords<BlockCompany>().ToList();

                // 若LIST不為空，才會將資料匯入DB
                if (DataList3.Count > 0)
                {
                    using var transaction = _context.Database.BeginTransaction();
                    _context.AddRange(DataList3);
                    _context.Database.ExecuteSqlRaw("SET IDENTITY_INSERT dbo.BlockCompany ON");
                    _context.SaveChanges();
                    _context.Database.ExecuteSqlRaw("SET IDENTITY_INSERT dbo.BlockCompany OFF");
                    transaction.Commit();
                }
            }

            // 若備份檔案存在，才會進一步讀取資料
            if (File.Exists(fPath4))
            {
                // 讀取檔案資料並轉成LIST
                using var reader4 = new StreamReader(fPath4, Encoding.UTF8);
                var csvReader4 = new CsvReader(reader4, CultureInfo.InvariantCulture);
                DataList4 = csvReader4.GetRecords<BlockForever>().ToList();

                // 若LIST不為空，才會將資料匯入DB
                if (DataList4.Count > 0)
                {
                    using var transaction = _context.Database.BeginTransaction();
                    _context.AddRange(DataList4);
                    _context.Database.ExecuteSqlRaw("SET IDENTITY_INSERT dbo.BlockForever ON");
                    _context.SaveChanges();
                    _context.Database.ExecuteSqlRaw("SET IDENTITY_INSERT dbo.BlockForever OFF");
                    transaction.Commit();
                }
            }
        }
    }
}
