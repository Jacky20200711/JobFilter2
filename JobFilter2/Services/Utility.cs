using System.Collections.Generic;

namespace JobFilter2.Services
{
    public static class Utility
    {
        /// <summary>
        /// 列舉封鎖公司的可能原因
        /// </summary>
        public static List<string> BlockReason = new List<string>
        {
            "派遣或駐點",
            "交通不方便",
            "技能樹不符合",
            "薪水太低",
            "搜尋評論後失去興趣",
            "上班時間多半小時",
            "對招募流程不滿意",
            "對這類產業沒興趣",
            "查看職缺後覺得很雷",
            "親自面過後失去興趣",
        };
    }
}
