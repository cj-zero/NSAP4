using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.ProductModel.Request
{
    public class AddProductModelCT4000
    {
        /// <summary>
        /// 序列号
        /// </summary>
        public string SerialNumber { get; set; }
        /// <summary>y
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
        /// 电压精度
        /// </summary>
        public string VoltageAccuracy { get; set; }
        /// <summary>
        /// 明细
        /// </summary>
        public AddProductModelInfoCT4000 Info { get; set; }
        
    }

    public class AddProductModelInfoCT4000
    {
        /// <summary>
        /// 最低放电电压
        /// </summary>
        
        public string MinimumDischargeVoltage { get; set; }
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
        /// 记录频率
        /// </summary>
       
        public string Fre { get; set; }
        /// <summary>
        /// 电压精度
        /// </summary>
      
        public string VoltAccurack { get; set; }
        /// <summary>
        /// 电压稳定度
        /// </summary>
       
        public string VoltageStability { get; set; }
      
        /// <summary>
        /// 电流稳定度
        /// </summary>
      
        public string CurrentStability { get; set; }
        /// <summary>
        /// 功率稳定度
        /// </summary>
     
        public string PowerStability { get; set; }
     
        /// <summary>
        /// 最小时间间隔
        /// </summary>
       
        public string MinimumTimeInterval { get; set; }
  
        /// <summary>
        /// 脉冲模式有无
        /// </summary>
     
        public string IsPulseMode { get; set; }
    }



}
