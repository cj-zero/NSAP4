using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.ProductModel
{
    public class ProductModelDetailsCT4000 : ExportBase
    {
        /// <summary>
        /// 设备编码
        /// </summary>
        public string DeviceCoding { get; set; }
        /// <summary>
        /// 输入电源
        /// </summary>
        public string InputPowerType { get; set; }
        /// <summary>
        /// 输入有功功率
        /// </summary>
        public string InputActivePower { get; set; }
        /// <summary>
        /// 通道
        /// </summary>
        public int ChannelNumber { get; set; }
        /// <summary>
        /// 恒压电压范围控制
        /// </summary>
        public string voltageRangeControl { get; set; }
        /// <summary>
        /// 最低放电电压
        /// </summary>
        public string MinimumDischargeVoltage { get; set; }
        /// <summary>
        /// 电压精度
        /// </summary>
        public string VoltageAccuracy { get; set; }
        /// <summary>
        /// 电压稳定度
        /// </summary>
        public string VoltageStability { get; set; }
        /// <summary>
        /// 电流每通道输出范围
        /// </summary>
        public string CurrentOutputRange { get; set; }
        /// <summary>
        /// 电流精度
        /// </summary>
        public string CurrentAccurack { get; set; }
        /// <summary>
        /// 恒压截至电流
        /// </summary>
        public string CutOffCurrent { get; set; }
        /// <summary>
        /// 电流稳定度
        /// </summary>

        public string CurrentStability { get; set; }

        /// <summary>
        /// 单通道输出最大功率
        /// </summary>
        public string SinglePowerMax { get; set; }
        /// <summary>
        /// 最小时间间隔
        /// </summary>
        public string MinimumTimeInterval { get; set; }
        /// <summary>
        /// 最小电压间隔
        /// </summary>
        public string MinimumVoltageInterval { get; set; }
        /// <summary>
        /// 最小电流间隔
        /// </summary>
        public string MinimumCurrentInterval { get; set; }
        /// <summary>
        /// 脉冲模式有无
        /// </summary>
        public string IsPulseMode { get; set; }
        /// <summary>
        /// 记录频率
        /// </summary>
        public string RecordFrequency { get; set; }


        /// <summary>
        /// 充电
        /// </summary>
        public string Charge { get; set; }

        /// <summary>
        /// 放电
        /// </summary>
        public string Discharge { get; set; }

        /// <summary>
        /// 最小脉冲宽度
        /// </summary>
        public string MinimumPulseWidth { get; set; }

        /// <summary>
        /// 脉冲个数
        /// </summary>
        public string NumberOfPulses { get; set; }

        /// <summary>
        /// 充放电
        /// </summary>
        public string ChargeAndDischarge { get; set; }

        /// <summary>
        /// 截止条件
        /// </summary>
        public string CutOffCondition { get; set; }



        ///// <summary>
        ///// 设备尺寸
        ///// </summary>
        //public string Size { get; set; }
        ///// <summary>
        ///// 产品图
        ///// </summary>
        //public string Pic { get; set; }

        ///// <summary>
        ///// 重量
        ///// </summary>
        //public string Weight { get; set; }

    }
}
