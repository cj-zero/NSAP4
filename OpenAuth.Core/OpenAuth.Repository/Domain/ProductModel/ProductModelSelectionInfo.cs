using OpenAuth.Repository.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace OpenAuth.Repository.Domain
{
    /// <summary>
    /// 产品模型详情
    /// </summary>
    [Table("Product_Model_Selection_Info")]
    public class ProductModelSelectionInfo : BaseEntity<int>
    {
        /// <summary>
        /// 设备Id
        /// </summary>
        public int ProductModelSelectionId { get; set; }
        /// <summary>
        /// 最低放电电压
        /// </summary>
        [Description("最低放电电压")]
        [MaxLength(20)]
        public string MinimumDischargeVoltage { get; set; }
        /// <summary>
        /// 输入电源
        /// </summary>
        [Description("输入电源")]
        [MaxLength(10)]
        public string InputPowerType { get; set; }
        /// <summary>
        /// 输入有功功率
        /// </summary>
        [Description("输入有功功率")]
        [MaxLength(10)]
        public string InputActivePower { get; set; }
        /// <summary>
        /// 输入电流
        /// </summary>
        [Description("输入电流")]
        [MaxLength(10)]
        public string InputCurrent { get; set; }
        /// <summary>
        /// 记录频率
        /// </summary>
        [Description("记录频率")]
        [MaxLength(10)]
        public string Fre { get; set; }
        /// <summary>
        /// 电压精度
        /// </summary>
        [Description("最小电流间隔")]
        [MaxLength(10)]
        public string VoltAccurack { get; set; }
        /// <summary>
        /// 是否删除
        /// </summary>
        [Description("是否删除")]
        public bool IsDelete { get; set; }
        /// <summary>
        /// 创建日期
        /// </summary>
        [Description("创建日期")]
        public DateTime CreateTime { get; set; }
        /// <summary>
        /// 创建人
        /// </summary>
        [Description("创建人")]
        [MaxLength(20)]
        public string CreateUser { get; set; }
        /// <summary>
        /// 更新时间
        /// </summary>
        [Description("更新时间")]
        public DateTime? UpdateTime { get; set; }
        /// <summary>
        /// 更新人
        /// </summary>
        [Description("更新人")]
        [MaxLength(20)]
        public string UpdateUser { get; set; }
        public override void GenerateDefaultKeyVal()
        {

        }
        public override bool KeyIsNull()
        {
            return Id == 0;
        }
    }
}
