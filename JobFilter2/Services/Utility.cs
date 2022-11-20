using System.Collections.Generic;
using System.Linq;

namespace JobFilter2.Services
{
    /// <summary>
    /// 這個類別用來放置共用函數與共用變數
    /// </summary>
    public static class Utility
    {
        /// <summary>
        /// 定義封鎖代號與對應的內文(DB只會儲存代號，不會儲存對應的內文)
        /// </summary>
        public static Dictionary<string, string> BLOCK_REASON = new Dictionary<string, string>
        {
            { "R01", "派遣/需要駐點或出差" },
            { "R03", "技能不符" },
            { "R04", "薪水太低" },
            { "R02", "交通不方便" },
            { "R05", "搜尋評論後失去興趣" },
            { "R08", "對這類產業沒有興趣" },
            { "R09", "看完職缺/應徵說明後失去興趣" },
            { "R13", "看完公司/福利介紹後失去興趣" },
            { "R07", "面試邀約/詢問後失去興趣" },
            { "R10", "面試後不想去/無聲卡/感謝函" },
            { "R11", "投遞履歷後沒有回應/感謝函" },
        };

        
        private static string radioItemStr;

        /// <summary>
        /// 根據事先定義的封鎖代號與內文，來設定 radio 對應的封鎖代碼/封鎖理由
        /// </summary>
        public static void SetRadioItems()
        {
            List<string> radioItems = new List<string>();
            BLOCK_REASON.ToList().ForEach(x => radioItems.Add($"\'{x.Key}\':\'{x.Value}\'"));
            radioItemStr = string.Join(',', radioItems);
        }

        /// <summary>
        /// 取得 radio 對應的封鎖代碼/封鎖理由，其所有的 key 與 value
        /// </summary>
        public static string GetRadioItems()
        {
            return radioItemStr;
        }
    }
}
