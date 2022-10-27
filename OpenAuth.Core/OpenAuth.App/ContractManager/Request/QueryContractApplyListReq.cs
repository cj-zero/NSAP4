using System;
using System.Collections.Generic;

namespace OpenAuth.App.Request
{
    public class QueryContractApplyListReq : PageReq
    {
        /// <summary>
        /// 排序字段
        /// </summary>
        public string SortName { get; set; }

        /// <summary>
        /// desc 降序，asc 升序
        /// </summary>
        public string SortOrder { get; set; }

        /// <summary>
        /// Id
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 合同申请单编号
        /// </summary>
        public string ContractNo { get; set; }

        /// <summary>
        /// 申请人Id
        /// </summary>
        public string CreateId { get; set; }

        /// <summary>
        /// 申请人
        /// </summary>
        public string CreateName { get; set; }

        /// <summary>
        /// 客户代码/名称
        /// </summary>
        public string CustomerCodeOrName { get; set; }

        /// <summary>
        /// 所属公司
        /// </summary>
        public string CompanyType { get; set; }

        /// <summary>
        /// 报价单号
        /// </summary>
        public string QuotationNo { get; set; }

        /// <summary>
        /// 销售单号
        /// </summary>
        public string SaleNo { get; set; }

        /// <summary>
        /// 当前节点/合同状态
        /// </summary>
        public string ContractStatus { get; set; }

        /// <summary>
        /// 开始日期
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// 结束日期
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// 项目名称
        /// </summary>
        public string ItemName { get; set; }

        /// <summary>
        /// 项目编号
        /// </summary>
        public string ItemNo { get; set; }

        /// <summary>
        /// 文件类型
        /// </summary>
        public List<string> FileTypes { get; set; }
    }

    public class QueryReCallContractReq
    {
        /// <summary>
        /// 合同申请单Id
        /// </summary>
        public string ContractId { get; set; }

        /// <summary>
        /// 撤回备注
        /// </summary>
        public string Remarks { get; set; }
    }

    public class ContractMsgHelp
    {
        /// <summary>
        /// 合同编号
        /// </summary>
        public string ContractNo { get; set; }

        /// <summary>
        /// 合同类型名称
        /// </summary>
        public string ContractTypeName { get; set; }

        /// <summary>
        /// 文件类型名称
        /// </summary>
        public string FileTypeName { get; set; }

        /// <summary>
        /// 创建人Id
        /// </summary>
        public string CreateId { get; set; }

        /// <summary>
        /// 申请人
        /// </summary>
        public string CreateName { get; set; }

        /// <summary>
        /// 部门名称
        /// </summary>
        public string DeptName { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public string CreateTime { get; set; }

        /// <summary>
        /// 所属公司
        /// </summary>
        public string CompanyName { get; set; }

        /// <summary>
        /// 合同状态
        /// </summary>
        public string ContractStatusName { get; set; }
    }

    public class ContractApplyMsgHelp
    {
        /// <summary>
        /// Id
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 销售报价单号
        /// </summary>
        public string QuotationNo { get; set; }

        /// <summary>
        /// 销售订单号
        /// </summary>
        public string SaleNo { get; set; }

        /// <summary>
        /// 合同编号
        /// </summary>
        public string ContractNo { get; set; }

        /// <summary>
        /// 合同类型
        /// </summary>
        public string ContractType { get; set; }

        /// <summary>
        /// 合同类型名称
        /// </summary>
        public string ContractTypeName { get; set; }

        /// <summary>
        /// 创建人Id
        /// </summary>
        public string CreateId { get; set; }

        /// <summary>
        /// 申请人
        /// </summary>
        public string CreateName { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public string CreateTime { get; set; }

        /// <summary>
        /// 所属公司
        /// </summary>
        public string CompanyType { get; set; }

        /// <summary>
        /// 公司名称
        /// </summary>
        public string CompanyName { get; set; }

        /// <summary>
        /// 合同状态
        /// </summary>
        public string ContractStatus { get; set; }

        /// <summary>
        /// 合同状态名称
        /// </summary>
        public string ContractStatusName { get; set; }
    }

    public class QueryContractDownLoadFilesReq : PageReq
    {
        /// <summary>
        /// 排序字段
        /// </summary>
        public string SortName { get; set; }

        /// <summary>
        /// desc 降序，asc 升序
        /// </summary>
        public string SortOrder { get; set; }

        /// <summary>
        /// 合同申请单Id
        /// </summary>
        public string ContractId { get; set; }
    }

    public class ContractDownLoadFiles
    {
        /// <summary>
        /// 文件类型名称
        /// </summary>
        public string FileTypeName { get; set; }

        /// <summary>
        /// 文件名称
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// 操作人
        /// </summary>
        public string CreateName { get; set; }

        /// <summary>
        /// 操作时间
        /// </summary>
        public string CreateTime { get; set; }

        /// <summary>
        /// 部门名称
        /// </summary>
        public string DeptName { get; set; }
    }

    public class ContractHistory
    {
        /// <summary>
        /// Id
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 合同申请单ID
        /// </summary>
        public string ContractApplyId { get; set; }

        /// <summary>
        /// 操作行为
        /// </summary>
        public string Action { get; set; }

        /// <summary>
        /// 操作人
        /// </summary>
        public string CreateUser { get; set; }

        /// <summary>
        /// 操作人Id
        /// </summary>
        public string CreateUserId { get; set; }

        /// <summary>
        /// 操作时间
        /// </summary>
        public System.DateTime? CreateTime { get; set; }

        /// <summary>
        /// 审批时长(分钟)
        /// </summary>
        public int? IntervalTime { get; set; }

        /// <summary>
        /// 审批结果
        /// </summary>
        public string ApprovalResult { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }

        /// <summary>
        /// 审批阶段
        /// </summary>
        public string ApprovalStage { get; set; }

        /// <summary>
        /// 部门名称
        /// </summary>
        public string DeptName { get; set; }
    }
}