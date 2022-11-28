using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Serve.Request
{
    
    public class QueryServiceOrderCompleteReq
    {
        /// <summary>
        /// App记录id
        /// </summary>
        public int id { get; set; }
        /// <summary>
        /// 呼叫主题编码多个逗号隔开
        /// </summary>
        public string solution_code { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int user_id { get; set; }
        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime? start_time { get; set; }
        /// <summary>
        /// 服务单数量
        /// </summary>
        public int service_order_number { get; set; }
    }

    public class QueryServiceOrderCompleteResp
    {
        /// <summary>
        /// App记录id
        /// </summary>
        public int id { get; set; }
        /// <summary>
        /// 完成服务单时间
        /// </summary>
        public DateTime? complete_time { get; set; }
        /// <summary>
        /// 完成服务单数量
        /// </summary>
        public int order_number { get; set; }
    }
}
