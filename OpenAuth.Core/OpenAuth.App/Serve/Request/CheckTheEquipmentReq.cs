using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace OpenAuth.App.Request
{
    public class CheckTheEquipmentReq
    {
        /// <summary>
        /// 服务单Id
        /// </summary>
        [Required]
        public int ServiceOrderId { get; set; }

        /// <summary>
        /// 核对失败工单Id集合
        /// </summary>
        public string ErrorWorkOrderIds { get; set; }

        /// <summary>
        /// 核对正确设备Id集合
        /// </summary>
        public string CheckWorkOrderIds { get; set; }

        /// <summary>
        /// 当前技术员Id
        /// </summary>
        [Required]
        public int CurrentUserId { get; set; }
    }
}
