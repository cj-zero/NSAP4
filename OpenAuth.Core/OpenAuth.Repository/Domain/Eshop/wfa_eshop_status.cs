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

namespace OpenAuth.Repository.Domain
{
    /// <summary>
	/// 
	/// </summary>
    [Table("wfa_eshop_status")]
    public partial class wfa_eshop_status
    {
        public wfa_eshop_status()
        {
            this.document_id = 0;
            this.job_id = 0;
            this.card_code = string.Empty;
            this.card_name = string.Empty;
            this.order_phase = string.Empty;
            this.shipping_phase = string.Empty;
            this.complete_phase = string.Empty;
            this.first_createdate = DateTime.Now;
        }

        /// <summary>
        /// 
        /// </summary>
        [Description("总合同状态主键")]
        public int document_id { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Description("报价单审批流编码")]
        public int job_id { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("提交用户编号")]
        public int? user_id { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("业务员编号")]
        public int? slp_code { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("相应报价单编号")]
        public int? quotation_entry { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("相应销售订单编号")]
        public int? order_entry { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("客户编码")]
        public string card_code { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("客户名称")]
        public string card_name { get; set; }
        /// <summary>
        /// 订单状态查询 (0 已提交 1 待发货 2 已发货 3 已完成）
        /// </summary>
        [Description("当前总状态")]
        public int? cur_status { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("提交阶段状态")]
        public string order_phase { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("交货阶段状态")]
        public string shipping_phase { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("结束阶段状态")]
        public string complete_phase { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("提交阶段最新更新时间")]
        public System.DateTime? order_lastdate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("交货阶段最新更新时间")]
        public System.DateTime? shipping_lastdate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("结束阶段最新更新时间")]
        public System.DateTime? complete_lastdate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("初次创建时间")]
        public System.DateTime? first_createdate { get; set; }

        [Description("报价单物料明细")]
        public virtual List<wfa_eshop_oqutdetail> wfa_eshop_oqutdetails { get; set; }

        [Description("销售单取消历史")]
        public virtual List<wfa_eshop_canceledstatus> wfa_eshop_canceledstatuss { get; set; }
    }
}