using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using OpenAuth.Repository.Core;
using Infrastructure.AutoMapper;


namespace OpenAuth.App.ContractManager.Request
{
    /// <summary>
    /// 合同文件类型表
    /// </summary>
    public partial class QueryContractFileTypeRequest : Entity
    {
        public QueryContractFileTypeRequest()
        {
            this.ContractApplyId = string.Empty;
            this.ContractSealId = string.Empty;
            this.FileType = string.Empty;
            this.ContractOriginalId = string.Empty;
            this.Remark = string.Empty;
        }

        /// <summary>
        /// 合同申请单Id
        /// </summary>
        [Description("合同申请单Id")]
        public string ContractApplyId { get; set; }

        /// <summary>
        /// 印章Id
        /// </summary>
        [Description("印章Id")]
        public string ContractSealId { get; set; }

        /// <summary>
        /// 文件类型
        /// </summary>
        [Description("文件类型")]
        public string FileType { get; set; }

        /// <summary>
        /// 合同原件Id
        /// </summary>
        [Description("合同原件Id")]
        public string ContractOriginalId { get; set; }

        /// <summary>
        /// 合同原件名称
        /// </summary>
        [Description("合同原件名称")]
        public string OriginalFileName { get; set; }

        /// <summary>
        /// 文件数量
        /// </summary>
        [Description("文件数量")]
        public int? FileNum { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        [Description("备注")]
        public string Remark { get; set; }

        /// <summary>
        /// 合同文件集合
        /// </summary>
        public virtual List<ContractFiles> children { get; set; }
    }

    public partial class ContractFiles : Entity
    {
        public ContractFiles()
        {
            this.ContractFileTypeId = string.Empty;
            this.FileId = string.Empty;
            this.CreateUploadId = string.Empty;
            this.CreateUploadTime = DateTime.Now;
        }

        /// <summary>
        /// 文件类型Id
        /// </summary>
        [Description("文件类型Id")]
        public string ContractFileTypeId { get; set; }

        /// <summary>
        /// 文件Id
        /// </summary>
        [Description("文件Id")]
        public string FileId { get; set; }


        /// <summary>
        /// 文件名称
        /// </summary>
        [Description("文件名称")]
        public string FileName { get; set; }

        /// <summary>
        /// 是否为最终合同
        /// </summary>
        [Description("是否为最终合同")]
        public bool IsFinalContract { get; set; }

        /// <summary>
        /// 是否盖章
        /// </summary>
        [Description("是否盖章")]
        public bool IsSeal { get; set; }

        /// <summary>
        /// 上传人
        /// </summary>
        [Description("上传人")]
        public string CreateUploadId { get; set; }

        /// <summary>
        /// 上传时间
        /// </summary>
        [Description("上传时间")]
        public System.DateTime? CreateUploadTime { get; set; }

        /// <summary>
        /// 更新人
        /// </summary>
        [Description("更新人")]
        public string UpdateUserId { get; set; }

        /// <summary>
        /// 更新时间
        /// </summary>
        [Description("更新时间")]
        public System.DateTime? UpdateUserTime { get; set; }
    }
}
