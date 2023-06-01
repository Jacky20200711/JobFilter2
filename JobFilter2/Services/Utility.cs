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
            { "R01", "技能不符" },
            { "R02", "薪水太低" },
            { "R03", "交通不方便" },
            { "R04", "搜尋評論後失去興趣" },
            { "R05", "對這類工作內容沒有興趣" },
            { "R06", "看完應徵說明後失去興趣" },
            { "R07", "了解福利與制度後失去興趣" },
            { "R08", "面試邀約或詢問後失去興趣" },
            { "R09", "面試後無聲卡或感謝函" },
            { "R10", "投遞履歷後沒有面試邀約" },
            { "R11", "面試體驗不佳" },
            { "R12", "職缺描述和實際內容不符" },
        };

        /// <summary>
        /// 取得 radio 的 Key 與 Value (這些值會填入前端 radio 的 inputOption 參數)
        /// </summary>
        public static string GetRadioItems()
        {
            List<string> radioItems = new List<string>();
            BLOCK_REASON.ToList().ForEach(x => radioItems.Add($"\'{x.Key}\':\'{x.Value}\'"));
            return string.Join(',', radioItems);
        }
    }
}
