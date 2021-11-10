using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.ProductModel
{
    public class ProductModelDetails
    {
        /// <summary>
        /// 设备编码
        /// </summary>
        public string DeviceCoding { get; set; }
        /// <summary>
        /// 通道
        /// </summary>
        public int ChannelNumber { get; set; }
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
        /// 整机效率
        /// </summary>
        public string Efficiency { get; set; }
        /// <summary>
        /// 噪声
        /// </summary>
        public string Noise { get; set; }
        /// <summary>
        /// 电压电流检测采样
        /// </summary>
        public string DeviceType { get; set; }
        /// <summary>
        /// 功率控制模块类型
        /// </summary>
        public string PowerControlModuleType { get; set; }
        /// <summary>
        /// 输入电源接线方式
        /// </summary>
        public string PowerConnection { get; set; }
        //电压-每通道测量范围
        /// <summary>
        /// 充电：
        /// </summary>
        public string ChargeVoltageRange { get; set; }
        /// <summary>
        /// 放电：
        /// </summary>
        public string DischargeVoltageRange { get; set; }
        /// <summary>
        /// 最低放电电压
        /// </summary>
        public string MinimumDischargeVoltage { get; set; }
        /// <summary>
        /// 电流-每通道测量范围 
        /// </summary>
        public string CurrentRange { get; set; }
        /// <summary>
        /// 精度（独立量程）
        /// </summary>
        public string CurrentAccurack { get; set; }
        /// <summary>
        /// 恒压截至电流
        /// </summary>
        public string CutOffCurrent { get; set; }
        /// <summary>
        /// 功率-单通道输出功率
        /// </summary>
        public string SinglePower { get; set; }

        /// <summary>
        /// 时间-电流响应时间
        /// </summary>
        public string CurrentResponseTime { get; set; }
        /// <summary>
        /// 电流转换时间
        /// </summary>
        public string CurrentConversionTime { get; set; }
        /// <summary>
        /// 数据记录-记录频率
        /// </summary>
        public string RecordFreq { get; set; }
        /// <summary>
        /// 最小时间电压间隔
        /// </summary>
        public string MinimumVoltageInterval { get; set; }
        /// <summary>
        /// 最新时间电流间隔
        /// </summary>
        public string MinimumCurrentInterval { get; set; }
        /// <summary>
        /// 整机输出功率
        /// </summary>
        public string TotalPower { get; set; }
        /// <summary>
        /// 设备尺寸
        /// </summary>
        public string Size { get; set; }
        /// <summary>
        /// 产品图
        /// </summary>
        public string Pic { get; set; }

        /// <summary>
        /// 重量
        /// </summary>
        public string Weight { get; set; }
        /// <summary>
        /// 电压精度
        /// </summary>
        public string VoltageAccuracy { get; set; }
    }
}
