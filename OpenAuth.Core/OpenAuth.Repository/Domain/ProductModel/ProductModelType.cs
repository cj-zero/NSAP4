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
    /// 产品模型类型
    /// </summary>
    [Table("Product_Model_Type")]
    public class ProductModelType : BaseEntity<int>
    {
        /// <summary>
        /// 分类Id
        /// </summary>
        [Description("分类Id")]
        [MaxLength(20)]
        public int ProductModelCategoryId { get; set; }
        /// <summary>
        /// 类型
        /// </summary>
        [Description("类型")]
        [MaxLength(20)]
        public string Name { get; set; }
        /// <summary>
        /// 产品手册
        /// </summary>
        [Description("产品手册")]
        [MaxLength(500)]
        public string ImageBanner { get; set; }
        /// <summary>
        /// 产品详情图片
        /// </summary>
        [Description("产品详情图片")]
        [MaxLength(500)]
        public string Image { get; set; }
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
