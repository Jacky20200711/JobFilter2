using CsvHelper;
using CsvHelper.Configuration;
using JobFilter2.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobFilter2.Services
{
    public class BackupService
    {
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

        public void Export(JobFilterContext _context, string exportPath)
        {
            // 切換路徑到目標資料夾
            Directory.SetCurrentDirectory(exportPath);

            // 撈出欲備份的資料
            List<CrawlSetting> DataList1 = _context.CrawlSettings.ToList();
            List<BlockJobItem> DataList2 = _context.BlockJobItems.ToList();
            List<BlockCompany> DataList3 = _context.BlockCompanies.ToList();

            // 將備份資料寫入CSV檔案
            using var writer1 = new StreamWriter("CrawlSettings.csv", false, Encoding.UTF8);
            using var csvWriter1 = new CsvWriter(writer1, CultureInfo.InvariantCulture);
            csvWriter1.WriteRecords(DataList1);

            using var writer2 = new StreamWriter("BlockJobItems.csv", false, Encoding.UTF8);
            using var csvWriter2 = new CsvWriter(writer2, CultureInfo.InvariantCulture);
            csvWriter2.WriteRecords(DataList2);

            using var writer3 = new StreamWriter("BlockCompanies.csv", false, Encoding.UTF8);
            using var csvWriter3 = new CsvWriter(writer3, CultureInfo.InvariantCulture);
            csvWriter3.WriteRecords(DataList3);
        }

        public void Import(JobFilterContext _context, string importPath)
        {

        }
    }
}
