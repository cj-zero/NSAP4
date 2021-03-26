using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Serve.Request
{
    public class AddDailyReportReq
    {
        /// <summary>
        /// 服务单Id
        /// </summary>
        public int ServiceOrderId { get; set; }

        /// <summary>
        /// 当前登录用户Id
        /// </summary>

        public int AppUserId { get; set; }

        /// <summary>
        /// 日报内容
        /// </summary>
        public List<DailyResult> dailyResults { get; set; }
    }

    public class DailyResult
    {
        /// <summary>
        /// 物料编码
        /// </summary>
        public string MaterialCode { get; set; }

        /// <summary>
        /// 序列号
        /// </summary>
        public string ManufacturerSerialNumber { get; set; }

        /// <summary>
        /// 问题描述
        /// </summary>
        public string TroubleDescription { get; set; }

        /// <summary>
        /// 解决方案
        /// </summary>
        public string ProcessDescription { get; set; }
    }
}
