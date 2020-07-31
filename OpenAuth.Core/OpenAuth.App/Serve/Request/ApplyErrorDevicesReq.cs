using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace OpenAuth.App.Serve.Request
{
    public class ApplyErrorDevicesReq
    {
        /// <summary>
        /// 服务单Id
        /// </summary>
        [Required]
        public int ServiceOrderId { get; set; }

        /// <summary>
        /// 技术员appUserId
        /// </summary>
        [Required]
        public int AppUserId { get; set; }

        /// <summary>
        /// 设备集合
        /// </summary>
        [Required]
        public virtual List<Device> Devices { get; set; }
    }
    /// <summary>
    /// 设备信息
    /// </summary>
    public class Device
    {
        public string manufacturerSerialNumber { get; set; }

        public string newNumber { get; set; }

        public string newCode { get; set; }
    }
}
