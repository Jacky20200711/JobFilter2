﻿using System.Collections.Generic;

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
            "技能樹不符合",
            "薪水未達期望",
            "搜尋評論後失去興趣",
            "上班時間9.5小時",
            "職缺描述寫太爛",
            "對招募流程不滿意",
            "查看職缺後失去興趣",
            "親自面過後失去興趣",
        };
    }
}
