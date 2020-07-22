using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace OpenAuth.App.Request
{
    public class GetAppTechnicianServiceWorkOrderReq : PageReq
    {
        /// <summary>
        /// 服务单Id
        /// </summary>
        [Required]
        public int ServiceOrderId { get; set; }

        /// <summary>
        /// 技术员Id
        /// </summary>
        [Required]
        public int TechnicianId { get; set; }

        /// <summary>
        /// 状态类型 1-未完成 2-已完成
        /// </summary>
        [Required]
        public int Type { get; set; }
    }
}
