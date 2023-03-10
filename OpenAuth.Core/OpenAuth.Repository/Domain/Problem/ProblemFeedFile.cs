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
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace OpenAuth.Repository.Domain
{
    /// <summary>
    /// 反馈附件表
    /// </summary>
    [Table("problemfeedfile")]
    public class ProblemFeedFile : BaseEntity<int>
    {
        public ProblemFeedFile()
        {
            this.FeedbackFileId = string.Empty;
        }

        /// <summary>
        /// 问题反馈Id
        /// </summary>
        [Description("问题反馈id")]
        public int? ProblemFeedbackId { get; set; }

        /// <summary>
        /// 反馈附件Id
        /// </summary>
        [Description("反馈附件Id")]
        public string FeedbackFileId { get; set; }

        public override void GenerateDefaultKeyVal()
        {

        }

        public override bool KeyIsNull()
        {
            return Id == 0;
        }
    }
}