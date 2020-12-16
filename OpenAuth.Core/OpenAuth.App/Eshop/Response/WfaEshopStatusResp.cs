using System;
using System.Collections.Generic;
using System.Text;
using Infrastructure.AutoMapper;
using OpenAuth.Repository.Domain;
using System.ComponentModel;

namespace OpenAuth.App.Response
{
    [AutoMapTo(typeof(wfa_eshop_status))]
    public class WfaEshopStatusResp
    {
        /// <summary>
        /// 
        /// </summary>
        [Description("报价单审批流编码")]
        public int job_id { get; set; }
        
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
        /// 
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
        
        [Description("销售单物流明细")]
        public virtual List<LogInfo> wfa_eshop_Logs { get; set; }

    }

    public class LogInfo
    {
        /// <summary>
        /// 
        /// </summary>
        [Description("物流公司名称")]
        public string CoName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("物流单号")]
        public string ExpNum { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("物流单上传时间")]
        public System.DateTime? CreateDate { get; set; }
    }
}
