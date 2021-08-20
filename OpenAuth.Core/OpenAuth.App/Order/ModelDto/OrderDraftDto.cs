using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Order.ModelDto
{
    /// <summary>
    /// 订单草稿详情
    /// </summary>
    public class OrderDraftInfo
    {
        /// <summary>
        /// 系统操作者
        /// </summary>
        public string U_YGMD { get; set; }
        /// <summary>
        /// 客户代码
        /// </summary>
        public string CardCode { get; set; }
        /// <summary>
        /// 客户名称
        /// </summary>
        public string CardName { get; set; }
        /// <summary>
        /// 联系人
        /// </summary>
        public string CntctCode { get; set; }
        /// <summary>
        /// 业务伙伴参考编号
        /// </summary>
        public string NumAtCard { get; set; }
        /// <summary>
        /// 币种  
        /// </summary>
        public string DocCur { get; set; }
        /// <summary>
        /// 汇率
        /// </summary>
        public string DocRate { get; set; }
        /// <summary>
        /// 订单Id
        /// </summary>
        public string DocNum { get; set; }
        /// <summary>
        /// 单据类型
        /// </summary>
        public string DocType { get; set; }
        /// <summary>
        /// 总计折扣
        /// </summary>
        public string DiscSum { get; set; }
        /// <summary>
        /// 折扣率
        /// </summary>
        public string DiscPrcnt { get; set; }
        /// <summary>
        /// 总计费用
        /// </summary>
        public string TotalExpns { get; set; }
        /// <summary>
        /// 税额总计
        /// </summary>
        public string VatSum { get; set; }
        /// <summary>
        /// 总价
        /// </summary>
        public string DocTotal { get; set; }
        /// <summary>
        /// 单据日期（创建时间）
        /// </summary>
        public string DocDate { get; set; }
        /// <summary>
        /// 起息日
        /// </summary>
        public string DocDueDate { get; set; }
        /// <summary>
        /// 单据日期
        /// </summary>
        public string TaxDate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string SupplCode { get; set; }
        /// <summary>
        /// 收货方
        /// </summary>
        public string ShipToCode { get; set; }
        /// <summary>
        /// 付款方
        /// </summary>
        public string PayToCode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Address { get; set; }
        public string Address2 { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string Comments { get; set; }
        /// <summary>
        /// 销售员
        /// </summary>
        public string SlpCode { get; set; }
        /// <summary>
        /// 装运类型
        /// </summary>
        public string TrnspCode { get; set; }
        /// <summary>
        /// 付款条件
        /// </summary>
        public string GroupNum { get; set; }
        /// <summary>
        /// 付款方式
        /// </summary>
        public string PeyMethod { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string VatPercent { get; set; }
        /// <summary>
        /// 国税编号
        /// </summary>
        public string LicTradNum { get; set; }
        /// <summary>
        /// 标识
        /// </summary>
        public string Indicator { get; set; }
        /// <summary>
        /// 部分交货
        /// </summary>
        public string PartSupply { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string ReqDate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string CANCELED { get; set; }
        /// <summary>
        /// 生产人
        /// </summary>
        public string U_ShipName { get; set; }
        /// <summary>
        /// 生产部门
        /// </summary>
        public string U_SCBM { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string DpmPrcnt { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Printed { get; set; }
        /// <summary>
        /// 订单状态
        /// </summary>
        public string DocStatus { get; set; }
        /// <summary>
        /// 所有者
        /// </summary>
        public string OwnerCode { get; set; }
        /// <summary>
        /// 发票
        /// </summary>
        public string U_FPLB { get; set; }
        /// <summary>
        /// 税率
        /// </summary>
        public string U_SL { get; set; }
        /// <summary>
        /// 业务员
        /// </summary>
        public string U_YWY { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string U_New_ORDRID { get; set; }
        /// <summary>
        /// 商城单号
        /// </summary>
        public string U_EshopNo { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string DocTotalFC { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string DiscSumFC { get; set; }
    }
    /// <summary>
    /// 文件
    /// </summary>
    public class OrderFile
    {
        /// <summary>
        /// Id
        /// </summary>
        public int file_id { get; set; }
        /// <summary>
        /// 类型
        /// </summary>
        public string type_name { get; set; }
        /// <summary>
        /// 附件名称
        /// </summary>
        public int file_nm { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public int remarks { get; set; }
        /// <summary>
        /// 路径
        /// </summary>
        public int file_path { get; set; }
        /// <summary>
        /// 上传时间
        /// </summary>
        public int upd_dt { get; set; }
        /// <summary>
        /// 操作者
        /// </summary>
        public int user_nm { get; set; }
        /// <summary>
        /// 浏览地址
        /// </summary>
        public string view_file_path { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int file_type_id { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int acct_id { get; set; }

    }

    /// <summary>
    /// 物料
    /// </summary>
    public class OrderItem
    {
        /// <summary>
        /// 物料描述
        /// </summary>
        public string Dscription { get; set; }
        public string Quantity { get; set; }
        public string PriceBefDi { get; set; }
        public string DiscPrcnt { get; set; }
        public string U_PDXX { get; set; }
        public string U_XSTCBL { get; set; }
        public string U_YWF { get; set; }
        public string U_FWF { get; set; }
        public string Price { get; set; }
        public string VatGroup { get; set; }
        public string PriceAfVAT { get; set; }
        public string LineTotal { get; set; }
        public string TotalFrgn { get; set; }
        public string U_SCTCBL { get; set; }
        public string StockPrice { get; set; }
        public string U_YF { get; set; }
        public string WhsCode { get; set; }
        public string OnHand { get; set; }
        public string VatPrcnt { get; set; }
        public string LineNum { get; set; }
        public string U_YFTCBL { get; set; }
        public string IsCommited { get; set; }
        public string OnOrder { get; set; }
        public string U_TDS { get; set; }
        public string U_DL { get; set; }
        public string U_DY { get; set; }
        public string DocEntry { get; set; }
        public string OpenQty { get; set; }
        public string U_JGF { get; set; }
        public string QryGroup1 { get; set; }
        public string QryGroup2 { get; set; }
        public string QryGroup3 { get; set; }
        public string U_YFCB { get; set; }
        public string U_SHJSDJ { get; set; }
        public string U_SHJSJ { get; set; }
        public string U_SHTC { get; set; }
        public string SumQuantity { get; set; }
        public string QryGroup8 { get; set; }
        public string QryGroup9 { get; set; }
        public string QryGroup10 { get; set; }
        public string buyunitmsr { get; set; }
        public string U_ZS { get; set; }
        public string U_RelDoc { get; set; }
        public string LineStatus { get; set; }
        public string BaseEntry { get; set; }
        public string BaseLine { get; set; }
        public string BaseType { get; set; }

    }
}
