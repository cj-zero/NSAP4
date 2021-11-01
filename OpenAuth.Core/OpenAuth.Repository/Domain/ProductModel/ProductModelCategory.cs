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
    /// 产品模型分类
    /// </summary>
    [Table("Product_Model_Category")]
    public class ProductModelCategory : BaseEntity<int>
    {
        /// <summary>
        /// 应用名称
        /// </summary>
        [Description("分类")]
        [MaxLength(20)]
        public string Category { get; set; }
        ///// <summary>
        ///// 产品系列
        ///// </summary>
        //[Description("产品系列")]
        //[MaxLength(20)]
        //public string Type { get; set; }
        /// <summary>
        /// 产品手册
        /// </summary>
        [Description("产品手册")]
        [MaxLength(500)]
        public string Image { get; set; }
        /// <summary>
        /// 产品案例
        /// </summary>
        [Description("产品案例")]
        [MaxLength(500)]
        public string CaseImage { get; set; }
        /// <summary>
        /// 规格说明书模板
        /// </summary>
        [Description("规格说明书模板")]
        [MaxLength(500)]
        public string SpecsDocTemplatePath { get; set; }
        /// <summary>
        /// 技术协议模板
        /// </summary>
        [Description("技术协议模板")]
        [MaxLength(500)]
        public string TAgreementDocTemplatePath { get; set; }
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
