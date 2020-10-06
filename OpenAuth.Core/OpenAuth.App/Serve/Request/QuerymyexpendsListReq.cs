using System;

namespace OpenAuth.App.Request
{
    public class QueryMyExpendsListReq : PageReq
    {
        /// <summary>
        /// appid
        /// </summary>
        public int? AppId { get; set; }
        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime? StartTime { get; set; }
        /// <summary>
        /// 结束时间
        /// </summary>
        public DateTime? EndTime { get; set; }
        //todo:添加自己的请求字段
    }
}