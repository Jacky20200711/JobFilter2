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
            "接案或派遣",
            "交通不方便",
            "技能樹不符合",
            "看完評論後失去興趣",
            "看完職缺描述或公司介紹後失去興趣",
            "看完面試說明或親自面過後失去興趣",
        };
    }
}
