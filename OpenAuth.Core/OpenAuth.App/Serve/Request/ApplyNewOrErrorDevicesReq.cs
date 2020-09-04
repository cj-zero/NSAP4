using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace OpenAuth.App.Serve.Request
{
    public class ApplyNewOrErrorDevicesReq
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
        /// 设备类型
        /// </summary>
        [Required]
        public string MaterialType { get; set; }

        /// <summary>
        /// 更正后的设备集合
        /// </summary>
        public virtual List<Device> Devices { get; set; }

        /// <summary>
        /// 新设备信息
        /// </summary>
        public virtual List<NewDevice> NewDevices { get; set; }
    }
    /// <summary>
    /// 设备信息
    /// </summary>
    public class Device
    {
        /// <summary>
        /// 工单号
        /// </summary>
        public int workOrderId { get; set; }

        /// <summary>
        /// 旧序列号
        /// </summary>
        public string manufacturerSerialNumber { get; set; }

        /// <summary>
        /// 新序列号
        /// </summary>
        public string newNumber { get; set; }

        /// <summary>
        /// 新型号
        /// </summary>
        public string newCode { get; set; }

        /// <summary>
        /// 保修结束日期
        /// </summary>
        public DateTime? dlvryDate { get; set; }

        /// <summary>
        /// 服务合同
        /// </summary>
        public string ContractId { get; set; }

        /// <summary>
        /// 物料描述
        /// </summary>
        public string ItemName { get; set; }

        /// <summary>
        /// 内部序列号
        /// </summary>
        public string InternalSN { get; set; }
    }

    /// <summary>
    /// 新添加设备信息
    /// </summary>
    public class NewDevice
    {
        /// <summary>
        /// 序列号
        /// </summary>

        public string manufacturerSerialNumber { get; set; }

        /// <summary>
        /// 型号
        /// </summary>
        public string ItemCode { get; set; }

        /// <summary>
        /// 保修结束日期
        /// </summary>
        public DateTime? dlvryDate { get; set; }

        /// <summary>
        /// 服务合同
        /// </summary>
        public string ContractId { get; set; }

        /// <summary>
        /// 物料描述
        /// </summary>
        public string ItemName { get; set; }

        /// <summary>
        /// 内部序列号
        /// </summary>
        public string InternalSN { get; set; }
    }
}
