using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace OpenAuth.App.Request
{
    public class BookingWorkOrderReq
    {
        /// <summary>
        /// 预约时间
        /// </summary>
        [Required]
        public DateTime BookingDate { get; set; }

        /// <summary>
        /// 服务Id
        /// </summary>
        [Required]
        public int ServiceOrderId { get; set; }

        /// <summary>
        /// 当前技术员Id
        /// </summary>
        [Required]
        public int CurrentUserId { get; set; }
    }
}
