//------------------------------------------------------------------------------
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
    /// 合同印章表
    /// </summary>
    [Table("contractseal")]
    public partial class ContractSeal : Entity
    {
        public ContractSeal()
        {
            this.SealNo = string.Empty;
            this.CompanyType = string.Empty;
            this.SealType = string.Empty;
            this.SealName = string.Empty;
            this.SealImageFileId = string.Empty;
            this.CreateUserId = string.Empty;
            this.CreateUserName = string.Empty;
            this.UpdateUserId = string.Empty;
            this.UpdateUserName = string.Empty;
            this.CreateTime = DateTime.Now;
            this.Remark = string.Empty;
        }

        /// <summary>
        /// 印章编号
        /// </summary>
        [Description("印章编号")]
        public string SealNo { get; set; }

        /// <summary>
        /// 所属公司
        /// </summary>
        [Description("所属公司")]
        public string CompanyType { get; set; }

        /// <summary>
        /// 印章类型
        /// </summary>
        [Description("印章类型")]
        public string SealType { get; set; }

        /// <summary>
        /// 印章名称
        /// </summary>
        [Description("印章名称")]
        public string SealName { get; set; }

        /// <summary>
        /// 印章图片Id
        /// </summary>
        [Description("印章图片Id")]
        public string SealImageFileId { get; set; }

        /// <summary>
        /// 是否启用
        /// </summary>
        [Description("是否启用")]
        public bool IsEnable { get; set; }

        /// <summary>
        /// 上传人Id
        /// </summary>
        [Description("上传人Id")]
        public string CreateUserId { get; set; }

        /// <summary>
        /// 上传人
        /// </summary>
        [Description("上传人")]
        public string CreateUserName { get; set; }

        /// <summary>
        /// 上传时间
        /// </summary>
        [Description("上传时间")]
        public System.DateTime? CreateTime { get; set; }

        /// <summary>
        /// 更新人Id
        /// </summary>
        [Description("更新人Id")]
        public string UpdateUserId { get; set; }

        /// <summary>
        /// 更新人
        /// </summary>
        [Description("更新人")]
        public string UpdateUserName { get; set; }

        /// <summary>
        /// 更新时间
        /// </summary>
        [Description("更新时间")]
        public System.DateTime? UpdateTime { get; set; }

        /// <summary>
        /// 印章说明
        /// </summary>
        [Description("印章说明")]
        public string Remark { get; set; }

        /// <summary>
        /// 盖章使用历史记录
        /// </summary>
        public virtual List<ContractSealOperationHistory> contractSealOperationHistoryList { get; set; }
    }
}