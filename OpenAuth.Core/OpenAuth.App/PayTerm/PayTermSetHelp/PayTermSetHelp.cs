using System.Collections.Generic;
using OpenAuth.Repository.Domain;

namespace OpenAuth.App.PayTerm.PayTermSetHelp
{
    public class PayTermHelp
    {
        /// <summary>
        /// Id
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 模块类型Id
        /// </summary>
        public int? ModuleTypeId { get; set; }

        /// <summary>
        /// 模块名称
        /// </summary>
        public string ModuleName { get; set; }

        /// <summary>
        /// 日期数值
        /// </summary>
        public decimal DateNumber { get; set; }

        /// <summary>
        /// 日期单位
        /// </summary>
        public string DateUnit { get; set; }

        /// <summary>
        /// 可选时间
        /// </summary>
        public string DateUnitName { get; set; }

        /// <summary>
        /// 是否默认
        /// </summary>
        public bool IsDefault { get; set; }

        /// <summary>
        /// 创建人Id
        /// </summary>
        public string CreateUserId { get; set; }

        /// <summary>
        /// 创建人
        /// </summary>
        public string CreateUser { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public System.DateTime? CreateDate { get; set; }

        /// <summary>
        /// 更新人Id
        /// </summary>
        public string UpdateUserId { get; set; }

        /// <summary>
        /// 更新人
        /// </summary>
        public string UpdateUser { get; set; }

        /// <summary>
        /// 更新时间
        /// </summary>
        public System.DateTime? UpdateDate { get; set; }

        /// <summary>
        /// 可选阶段名称
        /// </summary>
        public string PayPhaseName { get; set; }

        /// <summary>
        /// 支付阶段集合
        /// </summary>
        public List<PayPhase> PayPhases { get; set; }
    }

    public class PayPhaseHelp
    {
        /// <summary>
        /// 数值
        /// </summary>
        public decimal Number { get; set; }

        /// <summary>
        /// 单位
        /// </summary>
        public string Unit { get; set; }

        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 是否默认
        /// </summary>
        public bool IsDefault { get; set; }
    }

    public class PayPhaseDetailHelp
    { 
        /// <summary>
        /// 百分比
        /// </summary>
        public string Percentage { get; set; }

        /// <summary>
        /// 日期值
        /// </summary>
        public string DateNumber { get; set; }

        /// <summary>
        /// 期限值
        /// </summary>
        public string DateLimit { get; set; }
    }

    public class ReceDetailHelp
    {
        /// <summary>
        /// 应收发票号
        /// </summary>
        public string ReceInvoice { get; set; }

        /// <summary>
        /// 应收发票外币汇率
        /// </summary>
        public decimal? ReceDocRate { get; set; }

        /// <summary>
        /// 应收创建日期
        /// </summary>
        public string ReceCreateDate { get; set; }

        /// <summary>
        /// 应收发票总金额
        /// </summary>
        public string InvoiceTotalAmount { get; set; }

        /// <summary>
        /// 收款类型
        /// </summary>
        public string RecePayType { get; set; }

        /// <summary>
        /// 币种
        /// </summary>
        public string ReceDocCur { get; set; }

        /// <summary>
        /// 应收金额
        /// </summary>
        public string ReceAmount { get; set; }

        /// <summary>
        /// 应收类型日期
        /// </summary>
        public string ReceTypeDate { get; set; }

        /// <summary>
        /// 应收到期日
        /// </summary>
        public string ReceDate { get; set; }

        /// <summary>
        /// 实际收款日期
        /// </summary>
        public string ActualReceDate { get; set; }

        /// <summary>
        /// 销售收款单号
        /// </summary>
        public string SaleReceiptNo { get; set; }

        /// <summary>
        /// 已收金额
        /// </summary>
        public string ReceiveAmount { get; set; }

        /// <summary>
        /// 未收金额
        /// </summary>
        public string NoReceiveAmount { get; set; }

        /// <summary>
        /// 逾期天数
        /// </summary>
        public string WithinLimitDay { get; set; }

        /// <summary>
        /// 逾期金额（货币形式）
        /// </summary>
        public string WithinLimitAmount { get; set; }

        /// <summary>
        /// 逾期金额
        /// </summary>
        public decimal WithinAmount { get; set; }

        /// <summary>
        /// 逾期比例
        /// </summary>
        public string WithinLimitPre { get; set; }

        /// <summary>
        /// 收款情况
        /// </summary>
        public string AccountRece { get; set; }
    }

    public class ReceHelp
    {
        /// <summary>
        /// 应收发票号
        /// </summary>
        public string ReceInvoice { get; set; }

        /// <summary>
        /// 币种
        /// </summary>
        public string DocCur { get; set; }

        /// <summary>
        /// 应收发票外币汇率
        /// </summary>
        public decimal? ReceDocRate { get; set; }

        /// <summary>
        /// 应收创建日期
        /// </summary>
        public System.DateTime? ReceCreateDate { get; set; }

        /// <summary>
        /// 收款类型日期
        /// </summary>
        public string ReceTypeDate { get; set; }

        /// <summary>
        /// 应收发票总金额(人民币)
        /// </summary>
        public decimal? TotalAmount { get; set; }

        /// <summary>
        /// 应收发票总金额(外币)
        /// </summary>
        public decimal? TotalAmountFC { get; set; }

        /// <summary>
        /// 收款类型
        /// </summary>
        public string RecePayType { get; set; }

        /// <summary>
        /// 应收金额
        /// </summary>
        public decimal ReceAmount { get; set; }

        /// <summary>
        /// 应收金额(外币)
        /// </summary>
        public decimal ReceAmountFC { get; set; }

        /// <summary>
        /// 应收到期日
        /// </summary>
        public string ReceDate { get; set; }

        /// <summary>
        /// 应收日期
        /// </summary>
        public System.DateTime? ReceDateTime { get; set; }
    }

    public class SapEntityHelp
    {
        /// <summary>
        /// 单据编号
        /// </summary>
        public int? DocEntry{ get; set; }

        /// <summary>
        /// 源单据编号
        /// </summary>
        public int? BaseEntry { get; set; }

        /// <summary>
        /// 创建日期
        /// </summary>
        public System.DateTime? CreateDate { get; set; }

        /// <summary>
        /// 币种
        /// </summary>
        public string DocCur { get; set; }

        /// <summary>
        /// 总金额
        /// </summary>
        public decimal? TotalAmount { get; set; }

        /// <summary>
        /// 总金额(外币)
        /// </summary>
        public decimal? TotalAmountFC { get; set; }

        /// <summary>
        /// 外币汇率
        /// </summary>
        public decimal? DocRate { get; set; }

        /// <summary>
        /// 客户编码
        /// </summary>
        public string CardCode { get; set; }
    }

    public class SaleReceHelp
    { 
        /// <summary>
        /// 销售订单号
        /// </summary>
        public int? DocEntry { get; set; }

        /// <summary>
        /// 币种
        /// </summary>
        public string DocCur { get; set; }

        /// <summary>
        /// 汇率
        /// </summary>
        public string DocRate { get; set; }

        /// <summary>
        /// 销售订单应收总金额
        /// </summary>
        public string SaleReceAmount { get; set; }

        /// <summary>
        /// 销售订单应收总金额（外币）
        /// </summary>
        public string SaleReceAmountFC { get; set; }

        /// <summary>
        /// 应收发票总金额
        /// </summary>
        public string ReceAmount { get; set; }

        /// <summary>
        /// 应收发票总金额（外币）
        /// </summary>
        public string ReceAmountFC { get; set; }

        /// <summary>
        /// 实收金额
        /// </summary>
        public string ActualAmount { get; set; }

        /// <summary>
        /// 实收金额（外币）
        /// </summary>
        public string ActualAmountFC { get; set; }

        /// <summary>
        /// 最大逾期天数
        /// </summary>
        public string WithOutLimitMaxDay { get; set; }

        /// <summary>
        /// 逾期金额总计
        /// </summary>
        public string WithOutLimitAmount { get; set; }

        /// <summary>
        /// 逾期比例
        /// </summary>
        public string WithOutLimitPre { get; set; }
    }

    public class DocEntryGroupNumHelp
    { 
        /// <summary>
        /// 单据编号
        /// </summary>
        public int DocEntry { get; set; }

        /// <summary>
        /// 付款条件组编号
        /// </summary>
        public int GroupNum { get; set; }

        /// <summary>
        /// 付款条件组名称
        /// </summary>
        public string PymntGroup { get; set; }
    }

    public class SaleOrderDetailHelp
    {
        /// <summary>
        /// 应收明细实体数据集
        /// </summary>
        public List<ReceDetailHelp> ReceDetailHelps { get; set; }

        /// <summary>
        /// 总体情况实体
        /// </summary>
        public SaleReceHelp SaleReceHelp { get; set; }
    }
}
