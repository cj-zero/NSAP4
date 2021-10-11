using OpenAuth.App.Response;
using OpenAuth.Repository.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Workbench.Response
{
    /// <summary>
    /// 报销主表
    /// </summary>
    public class ReimburseDetailsResp
    {
        /// <summary>
        /// 报销单Id
        /// </summary>
        public int ReimburseId { get; set; }
        /// <summary>
        /// 报销单号
        /// </summary>
        public int ReimburseMainId { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }
        /// <summary>
        /// 总金额
        /// </summary>
        public decimal? TotalMoney { get; set; }
        /// <summary>
        /// 部门归属费用
        /// </summary>
        public decimal? OrgMoney { get; set; }
        /// <summary>
        /// 部门
        /// </summary>
        public string  Org { get; set; }
        /// <summary>
        /// 更新时间
        /// </summary>
        public System.DateTime UpdateTime { get; set; }
        /// <summary>
        /// 附件
        /// </summary>
        public List<FileResp> Files { get; set; }
        /// <summary>
        /// 流程
        /// </summary>
        public List<FlowPathResp> FlowPathResp { get; set; }
        /// <summary>
        /// 出差补贴
        /// </summary>
        public virtual List<ReimburseTravellingAllowanceResp> ReimburseTravellingAllowances { get; set; }
        /// <summary>
        /// 差旅报销其他费用
        /// </summary>
        public virtual List<ReimburseOtherChargesResp> ReimburseOtherCharges { get; set; }
        /// <summary>
        /// 差旅报销交通费用
        /// </summary>
        public virtual List<ReimburseFareResp> ReimburseFares { get; set; }
        /// <summary>
        /// 差旅报销住宿补贴
        /// </summary>
        public virtual List<ReimburseAccommodationSubsidyResp> ReimburseAccommodationSubsidies { get; set; }
        /// <summary>
        /// 差旅报销单操作历史
        /// </summary>
        public virtual List<OperationHistoryResp> ReimurseOperationHistories { get; set; }
    }
    /// <summary>
    /// 出差补贴
    /// </summary>
    public class ReimburseTravellingAllowanceResp 
    {
        /// <summary>
        /// 费用id
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// 序号
        /// </summary>
        public int? SerialNumber { get; set; }
        /// <summary>
        /// 天数
        /// </summary>
        public int? Days { get; set; }
        /// <summary>
        /// 金额
        /// </summary>
        public decimal? Money { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public string CreateTime { get; set; }

        /// <summary>
        /// 费用归属
        /// </summary>
        public string ExpenseOrg { get; set; }
        /// <summary>
        /// 费用类型
        /// </summary>
        public string ExpenseType { get; set; }
        /// <summary>
        /// 费用归属
        /// </summary>
        public List<ReimburseExpenseOrg> ReimburseExpenseOrgs { get; set; }

    }
    /// <summary>
    /// 差旅报销其他费用
    /// </summary>
    public class ReimburseOtherChargesResp
    {
        /// <summary>
        /// 费用id
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// 序号
        /// </summary>
        public int? SerialNumber { get; set; }
        /// <summary>
        /// 费用类别
        /// </summary>
        public string ExpenseCategory { get; set; }
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
        /// 我的费用Id
        /// </summary>
        public string MyExpendsId { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public string CreateTime { get; set; }

        /// <summary>
        /// 开票日期
        /// </summary>
        public string InvoiceTime { get; set; }

        /// <summary>
        /// 费用归属
        /// </summary>
        public string ExpenseOrg { get; set; }
        /// <summary>
        /// 费用类型
        /// </summary>
        public string ExpenseType { get; set; }

        /// <summary>
        /// 附件
        /// </summary>
        public List<FileResp> Files { get; set; }
        /// <summary>
        /// 费用归属
        /// </summary>
        public List<ReimburseExpenseOrg> ReimburseExpenseOrgs { get; set; }
    }
    /// <summary>
    /// 差旅报销交通费用
    /// </summary>
    public class ReimburseFareResp
    {
        /// <summary>
        /// 费用id
        /// </summary>
        public int Id { get; set; }
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
        /// 我的费用Id
        /// </summary>
        public string MyExpendsId { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public string CreateTime { get; set; }

        /// <summary>
        /// 开票日期
        /// </summary>
        public string InvoiceTime { get; set; }


        /// <summary>
        /// 出发地址经度
        /// </summary>
        public string FromLng { get; set; }

        /// <summary>
        /// 出发地址纬度
        /// </summary>
        public string FromLat { get; set; }

        /// <summary>
        /// 到达地址经度
        /// </summary>
        public string ToLng { get; set; }

        /// <summary>
        /// 到达地址纬度
        /// </summary>
        public string ToLat { get; set; }

        /// <summary>
        /// 费用归属
        /// </summary>
        public string ExpenseOrg { get; set; }
        /// <summary>
        /// 费用类型
        /// </summary>
        public string ExpenseType { get; set; }

        /// <summary>
        /// 附件
        /// </summary>
        public List<FileResp> Files { get; set; }
        /// <summary>
        /// 费用归属
        /// </summary>
        public List<ReimburseExpenseOrg> ReimburseExpenseOrgs { get; set; }
    }
    /// <summary>
    /// 差旅报销住宿补贴
    /// </summary>
    public class ReimburseAccommodationSubsidyResp
    {
        /// <summary>
        /// 费用id
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// 序号
        /// </summary>
        public int? SerialNumber { get; set; }
        /// <summary>
        /// 天数
        /// </summary>
        public int? Days { get; set; }
        /// <summary>
        /// 金额
        /// </summary>
        public decimal? Money { get; set; }
        /// <summary>
        /// 总金额
        /// </summary>
        public decimal? TotalMoney { get; set; }
        /// <summary>
        /// 发票号码
        /// </summary>
        public string InvoiceNumber { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }
        /// <summary>
        /// 我的费用Id
        /// </summary>
        public string MyExpendsId { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public string CreateTime { get; set; }
        
        /// <summary>
        /// 开票日期
        /// </summary>
        public string InvoiceTime { get; set; }

        /// <summary>
        /// 开票单位
        /// </summary>
        public string SellerName { get; set; }

        /// <summary>
        /// 费用归属
        /// </summary>
        public string ExpenseOrg { get; set; }
        /// <summary>
        /// 费用类型
        /// </summary>
        public string ExpenseType { get; set; }
        

        /// <summary>
        /// 附件
        /// </summary>
        public List<FileResp> Files { get; set; }
        /// <summary>
        /// 费用归属
        /// </summary>
        public List<ReimburseExpenseOrg> ReimburseExpenseOrgs { get; set; }
    }
}
