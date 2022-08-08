﻿//------------------------------------------------------------------------------
// <autogenerated>
//     This code was generated by a CodeSmith Template.
//
//     DO NOT MODIFY contents of this file. Changes to this
//     file will be lost if the code is regenerated.
//     Author:RenChun Xia
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
    /// 合同模板表
    /// </summary>
    [Table("contracttemplate")]
    public partial class ContractTemplate : Entity
    {
        public ContractTemplate()
        {
            this.CompanyType = string.Empty;
            this.TemplateNo = string.Empty;
            this.TemplateRemark = string.Empty;
            this.TemplateFileId = string.Empty;
            this.CreateUserId = string.Empty;
            this.CreateTime = DateTime.Now;
            this.UpdateUserId = string.Empty;
        }

        /// <summary>
        /// 所属公司
        /// </summary>
        [Description("所属公司")]
        public string CompanyType { get; set; }

        /// <summary>
        /// 模板编号
        /// </summary>
        [Description("模板编号")]
        public string TemplateNo { get; set; }

        /// <summary>
        /// 模板说明
        /// </summary>
        [Description("模板说明")]
        public string TemplateRemark { get; set; }

        /// <summary>
        /// 模板文件Id
        /// </summary>
        [Description("模板文件Id")]
        public string TemplateFileId { get; set; }

        /// <summary>
        /// 上传人
        /// </summary>
        [Description("上传人")]
        public string CreateUserId { get; set; }

        /// <summary>
        /// 上传时间
        /// </summary>
        [Description("上传时间")]
        public System.DateTime CreateTime { get; set; }

        /// <summary>
        /// 更新人
        /// </summary>
        [Description("更新人")]
        public string UpdateUserId { get; set; }

        /// <summary>
        /// 更新时间
        /// </summary>
        [Description("更新时间")]
        public System.DateTime? UpdateTime { get; set; }

        /// <summary>
        /// 下载次数
        /// </summary>
        [Description("下载次数")]
        public int DownLoadNum { get; set; }
    }
}