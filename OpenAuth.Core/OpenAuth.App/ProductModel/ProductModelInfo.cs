using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.ProductModel
{
    public class ProductModelInfo
    {
        public int Id { get; set; }
        /// <summary>
        /// 分类Id
        /// </summary>
        public int ProductModelCategoryId { get; set; }
        /// <summary>
        /// 序列号
        /// </summary>
        public string SerialNumber { get; set; }
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
        /// <summary>
        /// 电流精度
        /// </summary>
        public string CurrentAccurack { get; set; }
        /// <summary>
        /// 设备尺寸
        /// </summary>
        public string Size { get; set; }
        /// <summary>
        /// 重量
        /// </summary>
        public double Weight { get; set; }
        /// <summary>
        /// 价格
        /// </summary>
        public decimal UnitPrice { get; set; }
        /// <summary>
        /// 输入电源
        /// </summary>
        public string InputPowerType { get; set; }
        /// <summary>
        /// 输入有功功率
        /// </summary>
        public string InputActivePower { get; set; }
         /// <summary>
        /// 输入电流
        /// </summary>
        public string InputCurrent { get; set; }
        /// <summary>
        /// 最低放电电压
        /// </summary>
        public string MinimumDischargeVoltage { get; set; }
        /// <summary>
        /// 最小电流间隔
        /// </summary>
        public string VoltAccurack { get; set; }
    }
}
