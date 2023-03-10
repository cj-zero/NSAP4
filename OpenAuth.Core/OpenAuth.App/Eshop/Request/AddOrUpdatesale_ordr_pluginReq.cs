//------------------------------------------------------------------------------
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

namespace OpenAuth.App.Request
{
    /// <summary>
	/// 
	/// </summary>
    [Table("sale_ordr_plugin")]
    public partial class AddOrUpdatesale_ordr_pluginReq 
    {
        
        /// <summary>
        /// NSAP订单号
        /// </summary>
        public int? DocEntry { get; set; }
        /// <summary>
        /// Nsap账套ID
        /// </summary>
        public int? sbo_id { get; set; }
        
        /// <summary>
        /// 发票抬头 1 个人2 企业
        /// </summary>
        public int? InvoiceTitle { get; set; }
        /// <summary>
        /// 公司/个人名称
        /// </summary>
        public string InvoiceName { get; set; }
        /// <summary>
        /// 纳税人识别号
        /// </summary>
        public string InvoiceTaxSignNo { get; set; }
        /// <summary>
        /// 收件电话
        /// </summary>
        public string InvoiceReceivePhone { get; set; }
        /// <summary>
        /// 收件邮箱
        /// </summary>
        public string InvoiceReceiveEmail { get; set; }
        /// <summary>
        /// 收件地址
        /// </summary>
        public string InvoiceReceiveAddress { get; set; }

        //todo:添加自己的请求字段
        /// <summary>
        /// 商城订单编号
        /// </summary>
        public string EshopOrderNo { get; set; }
    }
}