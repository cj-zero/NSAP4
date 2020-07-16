using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace OpenAuth.App.Request
{
    public class TechnicianTakeOrderReq
    {
        /// <summary>
        /// 工单Id
        /// </summary>
        [Required]
        public int ServiceWorkOrderId { get; set; }

        /// <summary>
        /// 技术员Id
        /// </summary>
        [Required]
        public int TechnicianId { get; set; }
    }
}
