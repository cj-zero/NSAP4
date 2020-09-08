using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace OpenAuth.App.Serve.Request
{
    public class TransferOrdersReq
    {
        /// <summary>
        /// 服务单Id
        /// </summary>
        [Required]
        public string ServiceOrderId { get; set; }

        /// <summary>
        /// 设备类型
        /// </summary>
        [Required]
        public string MaterialType { get; set; }

        /// <summary>
        /// 转派技术员Id
        /// </summary>
        [Required]
        public int TechnicianId { get; set; }
    }
}
