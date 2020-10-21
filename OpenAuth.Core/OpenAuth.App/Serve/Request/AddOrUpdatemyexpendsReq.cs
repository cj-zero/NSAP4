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
using OpenAuth.App.Serve.Response;
using OpenAuth.Repository.Core;

namespace OpenAuth.App.Request
{
    /// <summary>
	/// 
	/// </summary>
    [Table("myexpends")]
    public partial class AddOrUpdateMyExpendsReq
    {

        /// <summary>
        /// 主键id
        /// </summary>
        public int? Id { get; set; }

        /// <summary>
        /// AppId
        /// </summary>
        public int? AppId { get; set; }
        /// <summary>
        /// 费用类型
        /// </summary>
        public string FeeType { get; set; }

        /// <summary>
        /// 报销单据类型(1 出差补贴， 2 交通费用， 3住宿补贴， 4 其他费用 5 其他费用)
        /// </summary>
        public int? ReimburseType { get; set; }
        /// <summary>
        /// 序号
        /// </summary>
        public int? SerialNumber { get; set; }
        /// <summary>
        /// 交通类型
        /// </summary>
        public string TrafficType { get; set; }
        /// <summary>
        /// 交通工具
        /// </summary>
        public string Transport { get; set; }
        /// <summary>
        /// 出发地
        /// </summary>
        public string From { get; set; }
        /// <summary>
        /// 目的地
        /// </summary>
        public string To { get; set; }
        /// <summary>
        /// 金额
        /// </summary>
        public decimal? Money { get; set; }
        /// <summary>
        /// 发票号码
        /// </summary>
        public string InvoiceNumber { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public System.DateTime CreateTime { get; set; }
        /// <summary>
        /// 天数
        /// </summary>
        public int? Days { get; set; }
        /// <summary>
        /// 总金额
        /// </summary>
        public decimal? TotalMoney { get; set; }
        /// <summary>
        /// 费用类别
        /// </summary>
        public string ExpenseCategory { get; set; }

        /// <summary>
        /// 是否删除
        /// </summary>
        public bool IsDelete { get; set; }

        /// <summary>
        /// 附件表
        /// </summary>
        public virtual List<ReimburseAttachmentResp> ReimburseAttachments { get; set; }


        /// <summary>
        /// 删除附件id
        /// </summary>
        public virtual List<string> fileid { get; set; }

        //todo:添加自己的请求字段
    }
}