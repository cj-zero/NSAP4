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
	/// 差旅报销单附件表
	/// </summary>
    [Table("reimburseattachment")]
    public partial class ReimburseAttachment : Entity
    {
        public ReimburseAttachment()
        {
          this.FileId= string.Empty;
        }


        /// <summary>
        /// 文件Id
        /// </summary>
        [Description("文件Id")]
        [Browsable(false)]
        public string FileId { get; set; }
        /// <summary>
        /// 关联表主键Id
        /// </summary>
        [Description("关联表主键Id")]
        [Browsable(false)]
        public int ReimburseId { get; set; }
        /// <summary>
        /// 报销单据类型(1 出差补贴， 2 交通费用， 3住宿补贴， 4 其他费用)
        /// </summary>
        [Description("报销单据类型")]
        public int ReimburseType { get; set; }
        /// <summary>
        /// 附件类型(1 普通附件， 2 发票附件)
        /// </summary>
        [Description("附件类型")]
        public int AttachmentType { get; set; }
    }
}