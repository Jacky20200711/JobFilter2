namespace JobFilter2.Services
{
    /// <summary>
    /// 這個類別用來放置共用函數與共用變數
    /// </summary>
    public static class Utility
    {
        /// <summary>
        /// 定義封鎖理由，這裡的值會連動到選擇封鎖理由的 selector
        /// </summary>
        public static string BLOCK_REASON = "'派遣或駐點': '派遣或駐點'," +
            "'交通不方便': '交通不方便'," +
            "'技能樹不符合': '技能樹不符合'," +
            "'薪水太低': '薪水太低'," +
            "'搜尋評論後失去興趣': '搜尋評論後失去興趣'," +
            "'表定時間超過9小時': '表定時間超過9小時'," +
            "'面試邀約/詢問後失去興趣': '面試邀約/詢問後失去興趣'," +
            "'對這類產業沒有興趣': '對這類產業沒有興趣'," +
            "'看完職缺說明後失去興趣': '看完職缺說明後失去興趣'," +
            "'面過後失去興趣/無聲卡': '面過後失去興趣/無聲卡'," +
            "'投遞履歷後沒有回應': '投遞履歷後沒有回應'," +
            "'莫名被發感謝函': '莫名被發感謝函',";
    }
}
