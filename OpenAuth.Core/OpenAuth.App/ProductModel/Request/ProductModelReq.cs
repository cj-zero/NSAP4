using OpenAuth.App.Request;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.ProductModel.Request
{
    /// <summary>
    /// 查询
    /// </summary>
    public class ProductModelReq : PageReq
    {
        /// <summary>
        /// 分类Id
        /// </summary>
        public int? ProductModelCategoryId { get; set; }
        /// <summary>
        /// 产品类型
        /// </summary>
        public string ProductType { get; set; }
        /// <summary>
        /// 设备编码
        /// </summary>
        public string DeviceCoding { get; set; }
        /// <summary>
        /// 电压
        /// </summary>
        public string Voltage { get; set; }
        /// <summary>
        /// 电流
        /// </summary>
        public string Current { get; set; }
        /// <summary>
        /// 序列号
        /// </summary>
        public int ChannelNumber { get; set; }
        /// <summary>
        /// 功率
        /// </summary>
        public string TotalPower { get; set; }
    }
}
