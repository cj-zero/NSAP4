using Infrastructure.Wrod;
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
        /// 通道数
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



    }

    public class ProductParamTemplate : ExportBase
    {
        public string DeviceCoding { get; set; }
        public string ChannelNumber { get; set; }
        public string InputPowerType { get; set; }
        public string InputActivePower { get; set; }
        public string InputCurrent { get; set; }
        public string Efficiency { get; set; }
        public string Noise { get; set; }
        public string DeviceType { get; set; }
        public string PowerControlModuleType { get; set; }
        public string PowerConnection { get; set; }
        public string ChargeVoltageRange { get; set; }
        public string DischargeVoltageRange { get; set; }
        public string MinimumDischargeVoltage { get; set; }
        public string CurrentRange { get; set; }
        public string CurrentAccurack { get; set; }
        public string CutOffCurrent { get; set; }
        public string SinglePower { get; set; }
        public string CurrentResponseTime { get; set; }
        public string CurrentConversionTime { get; set; }
        public string RecordFreq { get; set; }
        public string MinimumVoltageInterval { get; set; }
        public string MinimumCurrentInterval { get; set; }
        public string TotalPower { get; set; }
        public string Size { get; set; }
        public string Image { get; set; }
    }
}
