using System;

namespace OpenAuth.App.Request
{
    public class QueryDictionaryReq : PageReq
    {
        /// <summary>
        /// 中文
        /// </summary>
        public string Chinese { get; set; }

        /// <summary>
        /// 英文
        /// </summary>
        public string English { get; set; }

        /// <summary>
        /// 中文解释
        /// </summary>
        public string ChineseExplain { get; set; }

        /// <summary>
        /// 英文解释
        /// </summary>
        public string EnglishExplain { get; set; }

        /// <summary>
        /// 创建人
        /// </summary>
        public string CreateUser { get; set; }
    }
}
