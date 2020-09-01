using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Response
{
    public class ServerOrderStatListResp
    {
        /// <summary>
        /// 统计类型
        /// </summary>
        public string StatType { get; set; }
        /// <summary>
        /// 统计数据
        /// </summary>
        public List<ServiceOrderReportResp> StatList { get; set; }
    }
    
    public class ServiceOrderReportResp
    {
        /// <summary>
        /// 统计键
        /// </summary>
        public string StatId
        {
            get; set;
        }
        /// <summary>
        /// 统计键值对应显示名称
        /// </summary>
        public string StatName { get; set; }
        /// <summary>
        /// 服务数量
        /// </summary>
        public int ServiceCnt { get; set; }
    }
}
