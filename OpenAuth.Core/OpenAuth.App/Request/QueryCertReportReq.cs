using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Request
{
    public class QueryCertReportReq: PageReq
    {


        /// <summary>
        /// 校准报表类型   1=生产校准报表  2=出货校准报表
        /// </summary>
        public int Type { get; set; }
        /// <summary>
        /// 校准人
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 校准人Id(详情使用)
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 部门
        /// </summary>
        public string Org { get; set; }

        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime? StartTime { get; set; }
        /// <summary>
        /// 结束时间
        /// </summary>
        public DateTime? EndTime { get; set; }

        /// <summary>
        /// 校准完成开始时间
        /// </summary>
        public DateTime? CompleteStartTime { get; set; }
        /// <summary>
        /// 校准完成结束时间
        /// </summary>
        public DateTime? CompleteEndTime { get; set; }
        /// <summary>
        /// 序列号
        /// </summary>
        public string TesterSn { get; set; }
        /// <summary>
        /// 设别型号
        /// </summary>
        public string TesterModel { get; set; }
        /// <summary>
        /// 校准状态 OK-校准成功 NG-校准失败
        /// </summary>
        public string CalibrationStatus { get; set; }
    }
}
