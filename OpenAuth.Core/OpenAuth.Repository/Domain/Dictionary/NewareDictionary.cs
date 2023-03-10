//------------------------------------------------------------------------------
// <autogenerated>
//     This code was generated by a CodeSmith Template.
//
//     DO NOT MODIFY contents of this file. Changes to this
//     file will be lost if the code is regenerated.
//     Author:RenChun Xia
// </autogenerated>
//------------------------------------------------------------------------------
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
    /// 新威词典表
    /// </summary>
    [Table("newaredictionary")]
    public class NewareDictionary : BaseEntity<int>
    {
        /// <summary>
        /// 中文
        /// </summary>
        [Description("中文")]
        public string Chinese { get; set; }

        /// <summary>
        /// 英文
        /// </summary>
        [Description("英文")]
        public string English { get; set; }

        /// <summary>
        /// 中文解释
        /// </summary>
        [Description("中文解释")]
        public string ChineseExplain { get; set; }

        /// <summary>
        /// 英文解释
        /// </summary>
        [Description("英文解释")]
        public string EnglishExplain { get; set; }

        /// <summary>
        /// 创建人
        /// </summary>
        [Description("创建人")]
        public string CreateUser { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        [Description("创建时间")]
        public System.DateTime? CreateTime { get; set; }

        /// <summary>
        /// 更新时间
        /// </summary>
        [Description("更新时间")]
        public System.DateTime? UpdateTime { get; set; }

        public override void GenerateDefaultKeyVal()
        {
        }

        public override bool KeyIsNull()
        {
            return Id == 0;
        }
    }
}