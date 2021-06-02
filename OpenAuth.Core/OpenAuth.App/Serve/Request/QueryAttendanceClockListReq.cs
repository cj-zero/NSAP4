using System;
using System.Collections.Generic;

namespace OpenAuth.App.Request
{
    public class QueryAttendanceClockListReq : PageReq
    {
        //todo:添加自己的请求字段
        /// <summary>
        /// 姓名
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 部门Id
        /// </summary>
        public List<string> Org { get; set; }
        /// <summary>
        /// 拜访对象
        /// </summary>
        public string VisitTo { get; set; }
        /// <summary>
        /// 地址
        /// </summary>
        public string Location { get; set; }
        /// <summary>
        /// 打卡日期起点
        /// </summary>
        public DateTime? DateFrom { get; set; }
        /// <summary>
        /// 打卡日期结点
        /// </summary>
        public DateTime? DateTo { get; set; }
    }
}