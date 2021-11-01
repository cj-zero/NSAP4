using OpenAuth.Repository.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace OpenAuth.Repository.Domain.ProductModel
{
    /// <summary>
    /// 产品模型基本信息
    /// </summary>
    [Table("Product_Model_Selection")]
    public class ProductModelSelection : BaseEntity<int>
    {
        /// <summary>
        /// 分类Id
        /// </summary>
        public int ProductModelCategoryId { get; set; }
        /// <summary>
        /// 序列号
        /// </summary>
        [Description("序列号")]
        [MaxLength(50)]
        public string SerialNumber { get; set; }
        /// <summary>
        /// 产品类型
        /// </summary>
        [Description("产品类型")]
        [MaxLength(20)]
        public string ProductType { get; set; }
        /// <summary>
        /// 设备编码
        /// </summary>
        [Description("设备编码")]
        [MaxLength(20)]
        public string DeviceCoding { get; set; }
        /// <summary>
        /// 电压
        /// </summary>
        [Description("电压")]
        [MaxLength(10)]
        public string Voltage { get; set; }
        /// <summary>
        /// 电流
        /// </summary>
        [Description("电流")]
        [MaxLength(10)]
        public string Current { get; set; }
        /// <summary>
        /// 序列号
        /// </summary>
        [Description("序列号")]
        public int ChannelNumber { get; set; }
        /// <summary>
        /// 功率
        /// </summary>
        [Description("功率")]
        [MaxLength(10)]
        public string TotalPower { get; set; }
        /// <summary>
        /// 电流精度
        /// </summary>
        [Description("电流精度")]
        [MaxLength(10)]
        public string CurrentAccurack { get; set; }
        /// <summary>
        /// 设备尺寸
        /// </summary>
        [Description("设备尺寸")]
        [MaxLength(50)]
        public string Size { get; set; }
        /// <summary>
        /// 重量
        /// </summary>
        [Description("重量")]
        public double Weight { get; set; }
        /// <summary>
        /// 价格
        /// </summary>
        [Description("价格")]
        public decimal UnitPrice { get; set; }
        /// <summary>
        /// 序列号
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
        public DateTime UpdateUser { get; set; }
        /// <summary>
        /// 更新人
        /// </summary>
        [Description("更新人")]
        [MaxLength(20)]
        public string UpdateTime { get; set; }
        public override void GenerateDefaultKeyVal()
        {

        }

        public override bool KeyIsNull()
        {
            return Id == 0;
        }
    }
}
