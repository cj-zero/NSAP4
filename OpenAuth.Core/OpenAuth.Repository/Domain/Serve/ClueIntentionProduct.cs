using OpenAuth.Repository.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace OpenAuth.Repository.Domain.Serve
{
    /// <summary>
    /// 线索意向产品
    /// </summary>
    [Table("clueintentionproduct")]
    public class ClueIntentionProduct : BaseEntity<int>
    {
        /// <summary>
        /// 线索ID
        /// </summary>
        [Description("线索ID")]
        public int ClueId { get; set; }
        /// <summary>
        /// 图片
        /// </summary>
        [Description("图片")]
        public string Pic { get; set; }
        /// <summary>
        /// 物料编码
        /// </summary>
        [Description("物料编码")]
        public string ItemCode { get; set; }
        /// <summary>
        /// 物料名称
        /// </summary>
        [Description("物料名称")]
        public string ItemName { get; set; }
        /// <summary>
        /// 物料描述
        /// </summary>
        [Description("物料描述")]
        public string ItemDescription { get; set; }
        /// <summary>
        /// 商品卖点
        /// </summary>
        [Description("商品卖点")]
        public string CommoditySellingPoint { get; set; }
        /// <summary>
        /// 可用库存
        /// </summary>
        [Description("可用库存")]
        public string AvailableStock { get; set; }
        /// <summary>
        /// 成本单价
        /// </summary>
        [Description("成本单价")]
        public string UnitCost { get; set; }
        /// <summary>
        /// 创建人
        /// </summary>
        [Description("创建人")]
        public string CreateUser { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        [Description("创建时间")]
        public System.DateTime CreateTime { get; set; }
        /// <summary>
        /// 更新人
        /// </summary>
        [Description("更新人")]
        public string UpdateUser { get; set; }
        /// <summary>
        /// 更新时间
        /// </summary>
        [Description("更新时间")]
        public DateTime? UpdateTime { get; set; }
        /// <summary>
        /// 是否删除
        /// </summary>
        [Description("是否删除")]
        public bool IsDelete { get; set; } = false;

        public override void GenerateDefaultKeyVal()
        {

        }

        public override bool KeyIsNull()
        {
            return Id == 0;
        }
    }
}
