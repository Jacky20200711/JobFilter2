using System.Collections.Generic;

namespace JobFilter2.Services
{
    public static class Utility
    {
        /// <summary>
        /// 定義封鎖公司的可能原因
        /// </summary>
        public static List<string> BlockReason = new List<string>
        {
            "派遣或駐點",
            "交通不方便",
            "搜尋評論後失去興趣",
            "對該領域沒有興趣",
            "看公司名稱不順眼",
            "上班時間9.5小時",
            "看完職缺或公司的介紹後失去興趣",
            "收到邀約或親自面過後失去興趣",
        };
    }
}
