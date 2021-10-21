using OpenAuth.App.Request;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Meeting.Request
{
    public class MyCreatedLoadReq : PageReq
    {
        /// <summary>
        /// 单据号
        /// </summary>
        public int JobId { get; set; }
        /// <summary>
        /// 单据类型
        /// </summary>
        public string Type { get; set; }
        /// <summary>
        /// 状态
        /// </summary>
        public int Status { get; set; }
        /// <summary>
        /// 任务名称
        /// </summary>
        public string JobName { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }
        /// <summary>
        /// 源单号
        /// </summary>
        public int Base_entry { get; set; }

    }
}
