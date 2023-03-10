using NSAP.Entity.Sales;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Order.Request
{
    /// <summary>
    /// 销售报价单审核流程
    /// </summary>
    public class AuditResubmitReq
    {
        /// <summary>
        /// 单据号
        /// </summary>
        public int jobId { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string recommend { get; set; } = "\\";
        /// <summary>
        /// 操作类型
        /// agree：已批准
        /// reject：不批准
        /// pending：未决
        /// </summary>
        public string auditOpinionid { get; set; }
        /// <summary>
        ///是否更新
        ///默认： 1
        /// </summary>
        public string IsUpdate { get; set; }
        /// <summary>
        /// 默认：0
        /// </summary>
        public string vStock { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string Comments { get; set; }
        /// <summary>
        /// 审核备注
        /// </summary>
        public string Remark { get; set; }
        /// <summary>
        /// 自定义字段
        /// </summary>
        public string CustomFields { get; set; }
        /// <summary>
        /// 已选择的序列号列表
        /// </summary>
        public List<billSerialNumberChooseItem> ChoosedSerialNumberList { get; set; }
        /// <summary>
        /// 序列号列表
        /// </summary>
        public List<billSerialNumber> serialNumber { get; set; }
    }
}
