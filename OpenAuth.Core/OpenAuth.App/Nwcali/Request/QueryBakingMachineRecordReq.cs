using OpenAuth.App.Request;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Nwcali.Request
{
    /// <summary>
    /// 查询烤机记录
    /// </summary>
    public class QueryBakingMachineRecordReq: PageReq
    {
        /// <summary>
        /// 销售单号
        /// </summary>
        public int OriginAbs { get; set; }
        /// <summary>
        /// 生产编码
        /// </summary>
        public string GeneratorCode { get; set; }
        /// <summary>
        /// 烤机开始时间
        /// </summary>
        public DateTime? StartTime { get; set; }
        /// <summary>
        /// 烤机开始时间
        /// </summary>
        public DateTime? EndTime { get; set; }
        /// <summary>
        /// 物料编码
        /// </summary>
        public string ItemCode { get; set; }
        /// <summary>
        /// pcba sn码
        /// </summary>
        public string Sn { get; set; }
        /// <summary>
        /// 烤机结果 0:全部 1:通过  2:失败
        /// </summary>
        public int State { get; set; }
        /// <summary>
        /// 部门名字
        /// </summary>
        public string OrgName { get; set; }
        /// <summary>
        /// 操作人
        /// </summary>
        public string Operator { get; set; }
    }
}
