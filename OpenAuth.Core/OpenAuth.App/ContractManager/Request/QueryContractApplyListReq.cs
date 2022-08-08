using System;

namespace OpenAuth.App.Request
{
    public class QueryContractApplyListReq : PageReq
    {
        /// <summary>
        /// Id
        /// </summary>
        public string Id { get; set; }
      
        /// <summary>
        /// 合同申请单编号
        /// </summary>
        public string ContractNo { get; set; }

        /// <summary>
        /// 申请人
        /// </summary>
        public string CreateId { get; set; }

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
    }
}