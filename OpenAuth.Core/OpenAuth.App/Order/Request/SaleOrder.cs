using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace OpenAuth.App.Order.Request
{
    [Serializable]
    [DataContract]
    public class SaleOrder
    {
        [DataMember]
        public string DocNum { get; set; }
        /// <summary>
        /// 帐套ID
        /// </summary>
        [DataMember]
        public string SboId { get; set; }
        /// <summary>
        /// 用户ID
        /// </summary>
        [DataMember]
        public string UserId { get; set; }
        /// <summary>
        /// 系统操作者
        /// </summary>
        [DataMember]
        public string U_YGMD { get; set; }
        /// <summary>
        /// 业务员
        /// </summary>
        [DataMember]
        public string U_YWY { get; set; }
        /// <summary>
        /// 页面功能ID
        /// </summary>
        [DataMember]
        public string FuncId { get; set; }
        /// <summary>
        /// 凭证类型（I & S 默认值为I，I物料、S服务)
        /// </summary>
        [DataMember]
        public string DocType { get; set; }
        /// <summary>
        /// </summary>
        [DataMember]
        public string VatGroup { get; set; }

        /// <summary>
        /// 凭证状态（Y,N,默认值为N）
        /// </summary>
        [DataMember]
        public string Printed { get; set; }
        /// <summary>
        /// 凭证状态（O,C,默认值为O）
        /// </summary>

        [DataMember]
        public string DocStatus { get; set; }

        /// <summary>
        /// 过帐时间，由程序更新。
        /// </summary>
        [DataMember]
        public string DocDate { get; set; }

        /// <summary>
        /// 起息日
        /// </summary>
        [DataMember]
        public string DocDueDate { get; set; }

        /// <summary>
        /// 客户/供应商代码
        /// </summary>
        [DataMember]
        public string CardCode { get; set; }

        /// <summary>
        /// 客户/供应商名称
        /// </summary>
        [DataMember]
        public string CardName { get; set; }

        /// <summary>
        /// 收票方
        /// </summary>
        [DataMember]
        public string Address { get; set; }
        /// <summary>
        /// 运达方
        /// </summary>
        [DataMember]
        public string Address2 { get; set; }

        /// <summary>
        /// 税额总计
        /// </summary>
        [DataMember]
        public string VatSum { get; set; }
        /// <summary>
        /// 凭证折扣率
        /// </summary>
        [DataMember]
        public string DiscPrcnt { get; set; }
        /// <summary>
        /// 总计折扣
        /// </summary>
        [DataMember]
        public string DiscSum { get; set; }
        /// <summary>
        /// 总计折扣外币
        /// </summary>
        [DataMember]
        public string DiscSumFC { get; set; }
        /// <summary>
        /// 凭证货币
        /// </summary>
        [DataMember]
        public string DocCur { get; set; }
        /// <summary>
        /// 凭证汇率
        /// </summary>
        [DataMember]
        public string DocRate { get; set; }

        /// <summary>
        /// 凭证总计
        /// </summary>
        [DataMember]
        public string DocTotal { get; set; }
        /// <summary>
        /// 凭证总计外币
        /// </summary>
        [DataMember]
        public string DocTotalFC { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        [DataMember]
        public string Comments { get; set; }
        /// <summary>
        /// 审核备注
        /// </summary>
        [DataMember]
        public string Remark { get; set; }
        /// <summary>
        /// 付款条款代码
        /// </summary>
        [DataMember]
        public string GroupNum { get; set; }
        /// <summary>
        /// 销售代表
        /// </summary>
        [DataMember]
        public string SlpCode { get; set; }

        /// <summary>
        /// 所有者
        /// </summary>
        [DataMember]
        public string OwnerCode { get; set; }
        /// <summary>
        /// 交货方法
        /// </summary>
        [DataMember]
        public string TrnspCode { get; set; }
        /// <summary>
        /// 部分交货（Y代表Yes,N代表No,默认值为Y）
        /// </summary>
        [DataMember]
        public string PartSupply { get; set; }
        /// <summary>
        /// 是否通过银行（Y代表Yes,N代表No,默认值为Y）
        /// </summary>
        [DataMember]
        public string IsBank { get; set; }
        /// <summary>
        /// 联系人
        /// </summary>
        [DataMember]
        public string CntctCode { get; set; }
        /// <summary>
        /// 基础货币（L、S、C，默认值为C,其中L本币、C业务伙伴货币、S系统货币）
        /// </summary>
        [DataMember]
        public string CurSource { get; set; }
        /// <summary>
        /// 单据日期
        /// </summary>
        [DataMember]
        public string TaxDate { get; set; }
        /// <summary>
        /// 总计费用(运费)
        /// </summary>
        [DataMember]
        public string TotalExpns { get; set; }


        /// <summary>
        /// 标识
        /// </summary>
        [DataMember]
        public string Indicator { get; set; }

        /// <summary>
        /// 订单类型(0为销售,1为售后)
        /// </summary>
        [DataMember]
        public string BillDocType { get; set; }

        /// <summary>
        /// 运达方代码
        /// </summary>
        [DataMember]
        public string ShipToCode { get; set; }

        /// <summary>
        /// 许可的经销商号
        /// </summary>
        [DataMember]
        public string LicTradNum { get; set; }

        /// <summary>
        /// 付款方式
        /// </summary>
        [DataMember]
        public string PeyMethod { get; set; }
        /// <summary>
        /// 付款到
        /// </summary>
        [DataMember]
        public string PayToCode { get; set; }
        /// <summary>
        /// 业务伙伴参考编号
        /// </summary>
        [DataMember]
        public string NumAtCard { get; set; }
        /// <summary>
        /// 折扣前总计
        /// </summary>
        [DataMember]
        public string BeforeDiscSum { get; set; }
        /// <summary>
        /// 发票类别
        /// </summary>
        [DataMember]
        public string U_FPLB { get; set; }
        /// <summary>
        /// 税率
        /// </summary>
        [DataMember]
        public string U_SL { get; set; }

        /// <summary>
        /// 货到付百分比
        /// </summary>
        [DataMember]
        public string GoodsToPro { get; set; }
        /// <summary>
        /// 货到付款日期
        /// </summary>
        [DataMember]
        public string GoodsToDate { get; set; }

        /// <summary>
        /// 货到付款天数
        /// </summary>
        [DataMember]
        public string GoodsToDay { get; set; }
        /// <summary>
        /// 发货前付
        /// </summary>
        [DataMember]
        public string PayBefShip { get; set; }
        /// <summary>
        /// 预付百分比
        /// </summary>
        [DataMember]
        public string PrepaPro { get; set; }
        /// <summary>
        /// 预付款日期
        /// </summary>
        [DataMember]
        public string PrepaData { get; set; }
        /// <summary>
        /// 自定义字段
        /// </summary>
        [DataMember]
        public string CustomFields { get; set; }
        /// <summary>
        /// WhsCode
        /// </summary>
        [DataMember]
        public string WhsCode { get; set; }
        /// <summary>
        /// 单据类型
        /// </summary>
        [DataMember]
        public string billBaseType { get; set; }
        /// <summary>
        /// 单据单号
        /// </summary>
        [DataMember]
        public string billBaseEntry { get; set; }
        /// <summary>
        /// 未清数量
        /// </summary>
        [DataMember]
        public string OpenQry { get; set; }
        [DataMember]
        public string ReqDate { get; set; }

        /// <summary>
        /// 服务呼叫
        /// </summary>
        [DataMember]
        public string U_CallID { get; set; }
        [DataMember]
        public string U_CallName { get; set; }
        [DataMember]
        public string U_SerialNumber { get; set; }

        /// <summary>
        /// 退货基于单号
        /// </summary>
        [DataMember]
        public string D_Num { get; set; }

        /// <summary>
        /// 财务是否需要审核
        /// </summary>
        [DataMember]
        public string FinanceIsAuditCheck { get; set; }
        /// <summary>
        /// 业务费（总金额）
        /// </summary>
        [DataMember]
        public string U_YWFMoney { get; set; }
        /// <summary>
        /// 服务费（总金额）
        /// </summary>
        [DataMember]
        public string U_FWFMoney { get; set; }
        /// <summary>
        /// 销售提成（总金额）
        /// </summary>
        [DataMember]
        public string U_XSTCMoney { get; set; }
        /// <summary>
        /// 扣减费用（总金额）
        /// </summary>
        [DataMember]
        public string U_KJFYMoney { get; set; }
        /// <summary>
        /// 补助金额（总金额）
        /// </summary>
        [DataMember]
        public string U_BZJEMoney { get; set; }

        /// <summary>
        /// 售后结算价（总金额）
        /// </summary>
        [DataMember]
        public string U_SHJSJMoney { get; set; }
        /// <summary>
        /// 税额（总金额）
        /// </summary>
        [DataMember]
        public string U_TaxMoney { get; set; }
        [DataMember]
        public string U_New_ORDRID { get; set; }
        [DataMember]
        public string U_CancelReason { get; set; }
        /// <summary>
        /// 如果是商城订单记录商城单号
        /// </summary>
        [DataMember]
        public string U_EshopNo { get; set; }

        /// <summary>
        /// 单据关联单号
        /// </summary>
        [DataMember]
        public string U_RelDoc { get; set; }
        /// <summary>
        /// 标识订单来源，4是从ERP 4.0同步而来，3是默认来源于ERP 3.0， 2是商城已付款订单同步而来 
        /// </summary>
        [DataMember]
        public string U_ERPFrom { get; set; }

        /// <summary>
        /// 商城订单付款交易号 
        /// </summary>
        [DataMember]
        public string PayTransactionId { get; set; }
        /// <summary>
        /// 订单收货状态 1已签收 0 未签收
        /// </summary>
        [DataMember]
        public string U_ShipStatus { get; set; }
        /// <summary>
        /// 订单收完款状态 1已收款 0 未收款
        /// </summary>
        [DataMember]
        public string U_ReceivePayStatus { get; set; }
        /// <summary>
        /// 行明细
        /// </summary>
        [DataMember]
        public IList<OrderDetails> billSalesDetails { get; set; }
        /// <summary>
        /// 序列号
        /// </summary>
        [DataMember]
        public IList<SerialNumber> serialNumber { get; set; }
        /// <summary>
        /// 附件
        /// </summary>
        [DataMember]
        public IList<billAttchment> attachmentData { get; set; }
        /// <summary>
        /// 科目
        /// </summary>
        [DataMember]
        public IList<SalesAcctCode> billSalesAcctCode { get; set; }
        /// <summary>
        /// 科目
        /// </summary>
        [DataMember]
        public IList<DeliveryItemAid> billDeliveryItemAid { get; set; }
        /// <summary>
        /// 来料检查记录
        /// </summary>
        [DataMember]
        public IList<IQCDetail> IQCDetails { get; set; }
        /// 服务号
        /// </summary>
        [DataMember]
        public string ServiceID { get; set; }

        /// 序列号
        /// </summary>
        [DataMember]
        public string ManufSN { get; set; }

        /// 销售单号
        /// </summary>
        [DataMember]
        public string SaleDoc { get; set; }
        /// 运费
        /// </summary>
        [DataMember]
        public string Freight { get; set; }
        /// 运单号
        /// </summary>
        [DataMember]
        public string TrnspNO { get; set; }
        /// <summary>
        /// 业务伙伴是否是运输公司
        /// </summary>
        [DataMember]
        public string IsTransport { get; set; }

    }
    [Serializable]
    [DataContract]
    public class billDeliveryItemAid
    {
        [DataMember]
        public string ItemCode { get; set; }

        [DataMember]
        public string Times1 { get; set; }

        [DataMember]
        public string Times2 { get; set; }

        [DataMember]
        public string Times3 { get; set; }
        [DataMember]
        public string Parent { get; set; }
    }
}
