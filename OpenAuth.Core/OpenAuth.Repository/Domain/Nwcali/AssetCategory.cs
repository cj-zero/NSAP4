﻿//------------------------------------------------------------------------------
// <autogenerated>
//     This code was generated by a CodeSmith Template.
//
//     DO NOT MODIFY contents of this file. Changes to this
//     file will be lost if the code is regenerated.
//     Author:Yubao Li
// </autogenerated>
//------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using OpenAuth.Repository.Core;

namespace OpenAuth.Repository.Domain
{
    /// <summary>
	/// 资产类别详细数据表
	/// </summary>
    [Table("AssetCategory")]
    public partial class AssetCategory : BaseEntity<int>
    {
        public AssetCategory()
        {
          this.CategoryNumber= string.Empty;
          this.CategoryType= string.Empty;
        }

        
        /// <summary>
        /// 资产ID
        /// </summary>
        [Description("资产ID")]
        public int? AssetId { get; set; }
        /// <summary>
        /// 序号
        /// </summary>
        [Description("序号")]
        public string CategoryNumber { get; set; }
        /// <summary>
        /// 阻值
        /// </summary>
        [Description("阻值")]
        public decimal? CategoryOhms { get; set; }
        /// <summary>
        /// 不确定度
        /// </summary>
        [Description("不确定度")]
        public decimal? CategoryNondeterminacy { get; set; }
        /// <summary>
        /// 不确定类型
        /// </summary>
        [Description("不确定类型")]
        public string CategoryType { get; set; }
        /// <summary>
        /// 包含因子K
        /// </summary>
        [Description("包含因子K")]
        public decimal? CategoryBHYZ { get; set; }
        /// <summary>
        /// 排序
        /// </summary>
        [Description("排序")]
        public int? CategoryAort { get; set; }

        public override void GenerateDefaultKeyVal()
        {

        }

        public override bool KeyIsNull()
        {
            return Id == 0;
        }
    }
}