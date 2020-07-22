using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace OpenAuth.App.Request
{
    public class TechnicianServiceWorkOrderReq : PageReq
    {
        /// <summary>
        /// 技术员Id
        /// </summary>
        [Required]
        public int TechnicianId { get; set; }

        /// <summary>
        /// 状态类型 1-已完成 2-未完成
        /// </summary>
        [Required]
        public int Type { get; set; }

        /// <summary>
        /// 经度
        /// </summary>
        public decimal Longitude { get; set; }
        /// <summary>
        /// 纬度
        /// </summary>
        public decimal Latitude { get; set; }
    }
}
