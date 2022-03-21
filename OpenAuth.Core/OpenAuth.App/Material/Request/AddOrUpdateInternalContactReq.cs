using Infrastructure.AutoMapper;
using OpenAuth.App.Request;
using OpenAuth.Repository.Domain;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace OpenAuth.App.Material.Request
{
    /// <summary>
    /// 内部联络单
    /// </summary>
    [Table("InternalContact")]
    [AutoMapTo(typeof(InternalContact))]
    public partial class AddOrUpdateInternalContactReq
    {
        /// <summary>
        /// ID
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// IW号
        /// </summary>
        public string IW { get; set; }
        public int? Types { get; set; }

        /// <summary>
        /// 主题
        /// </summary>
        public string Theme { get; set; }
        /// <summary>
        /// 客户编号
        /// </summary>
        public List<string> CardCodes { get; set; }
        /// <summary>
        /// 客户名称
        /// </summary>
        public List<string> CardNames { get; set; }
        /// <summary>
        /// 状态
        /// </summary>
        public int? Status { get; set; }
        /// <summary>
        /// rdms项目号
        /// </summary>
        public string RdmsNo { get; set; }
        /// <summary>
        /// 销售单号
        /// </summary>
        public List<string> SaleOrderNo { get; set; }
        /// <summary>
        /// 生产单号
        /// </summary>
        public string ProductionNo { get; set; }
        /// <summary>
        /// 适应型号
        /// </summary>
        public string AdaptiveModel { get; set; }
        /// <summary>
        /// 适应范围
        /// </summary>
        public List<string> AdaptiveRanges { get; set; }
        /// <summary>
        /// 变更原因
        /// </summary>
        public List<string> Reasons { get; set; }
        /// <summary>
        /// 物料生成订单
        /// </summary>
        public List<string> MaterialOrder { get; set; }
        /// <summary>
        /// 测试审批人ID
        /// </summary>
        public string CheckApproveId { get; set; }
        /// <summary>
        /// 测试审批人名称
        /// </summary>
        public string CheckApprove { get; set; }
        /// <summary>
        /// 研发审批人ID
        /// </summary>
        public string DevelopApproveId { get; set; }
        /// <summary>
        /// 研发审批名称
        /// </summary>
        public string DevelopApprove { get; set; }
        /// <summary>
        /// 变更内容
        /// </summary>
        public string Content { get; set; }
        /// <summary>
        /// 审批日期
        /// </summary>
        public System.DateTime? ApproveTime { get; set; }
        /// <summary>
        /// 执行日期
        /// </summary>
        public System.DateTime? ExecTime { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public System.DateTime? CreateTime { get; set; }
        /// <summary>
        /// 是否草稿
        /// </summary>
        public bool IsDraft { get; set; }
        /// <summary>
        /// 创建人ID
        /// </summary>
        public string CreateUserId { get; set; }
        /// <summary>
        /// 流程ID
        /// </summary>
        public string FlowInstanceId { get; set; }
        /// <summary>
        /// 创建人名称
        /// </summary>
        public string CreateUser { get; set; }
        /// <summary>
        /// 附件
        /// </summary>
        public List<string> Attchments { get; set; }
        /// <summary>
        /// 批次号
        /// </summary>
        //public List<InternalContactBatchNumberReq> InternalContactBatchNumbers { get; set; }
        /// <summary>
        /// 联络单物料
        /// </summary>
        //public List<InternalContactMaterialReq> InternalContactMaterials { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public List<InternalContactDeptInfoReq> InternalContactExecDepts { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public List<InternalContactDeptInfoReq> InternalContactReceiveDepts { get; set; }
        /// <summary>
        /// 任务单
        /// </summary>
        public List<InternalContactTaskReq> InternalContactTasks { get; set; }
        /// <summary>
        /// 服务呼叫
        /// </summary>
        public List<InternalContactServiceOrderReq> InternalContactServiceOrders { get; set; }
        /// <summary>
        /// 生产订单
        /// </summary>
        public List<InternalContactProductionReq> InternalContactProductions { get; set; }
        public List<QueryProductionOrderReq> MaterialInfo { get; set; }
    }
}
