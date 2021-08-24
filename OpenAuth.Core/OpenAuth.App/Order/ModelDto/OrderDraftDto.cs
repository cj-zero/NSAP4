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
        public int CntctCode { get; set; }
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
        public decimal DocRate { get; set; }
        /// <summary>
        /// 订单Id
        /// </summary>
        public int DocNum { get; set; }
        /// <summary>
        /// 单据类型
        /// </summary>
        public string DocType { get; set; }
        /// <summary>
        /// 总计折扣
        /// </summary>
        public decimal DiscSum { get; set; }
        /// <summary>
        /// 折扣率
        /// </summary>
        public decimal DiscPrcnt { get; set; }
        /// <summary>
        /// 总计费用
        /// </summary>
        public decimal TotalExpns { get; set; }
        /// <summary>
        /// 税额总计
        /// </summary>
        public decimal VatSum { get; set; }
        /// <summary>
        /// 总价
        /// </summary>
        public decimal DocTotal { get; set; }
        /// <summary>
        /// 单据日期（创建时间）
        /// </summary>
        public DateTime DocDate { get; set; }
        /// <summary>
        /// 货到付款日期
        /// </summary>
        public DateTime DocDueDate { get; set; }
        /// <summary>
        ///  预付款日期
        /// </summary>
        public DateTime TaxDate { get; set; }
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
        public int SlpCode { get; set; }
        /// <summary>
        /// 装运类型
        /// </summary>
        public int TrnspCode { get; set; }
        /// <summary>
        /// 付款条件
        /// </summary>
        public int GroupNum { get; set; }
        /// <summary>
        /// 付款方式
        /// </summary>
        public string PeyMethod { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public decimal VatPercent { get; set; }
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
        public DateTime ReqDate { get; set; }
        /// <summary>
        /// 是否已取消
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
        public decimal DpmPrcnt { get; set; }
        /// <summary>
        /// 打印状态
        /// </summary>
        public string Printed { get; set; }
        /// <summary>
        /// 订单状态
        /// </summary>
        public string DocStatus { get; set; }
        /// <summary>
        /// 经理
        /// </summary>
        public int OwnerCode { get; set; }
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
        /// 拟取消订单
        /// </summary>
        public string U_New_ORDRID { get; set; }
        /// <summary>
        /// 商城单号
        /// </summary>
        public string U_EshopNo { get; set; }
        /// <summary>
        /// 总计
        /// </summary>
        public decimal DocTotalFC { get; set; }
        /// <summary>
        /// 折扣前总计
        /// </summary>
        public decimal DiscSumFC { get; set; }
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
    public class OrderItemInfo
    {
        /// <summary>
        /// 物料编码
        /// </summary>
        public string ItemCode { get; set; }
        /// <summary>
        /// 物料描述
        /// </summary>
        public string Dscription { get; set; }
        /// <summary>
        /// 数量
        /// </summary>
        public int Quantity { get; set; }
        /// <summary>
        /// 单价
        /// </summary>
        public decimal PriceBefDi { get; set; }
        /// <summary>
        /// 折扣
        /// </summary>
        public decimal DiscPrcnt { get; set; }
        /// <summary>
        /// 配电选项
        /// </summary>
        public string U_PDXX { get; set; }
        /// <summary>
        /// 销售提成
        /// </summary>
        public decimal U_XSTCBL { get; set; }
        /// <summary>
        /// 差旅费
        /// </summary>
        public decimal U_YWF { get; set; }
        /// <summary>
        /// 服务费
        /// </summary>
        public decimal U_FWF { get; set; }
        /// <summary>
        /// 折扣后价格
        /// </summary>
        public decimal Price { get; set; }
        /// <summary>
        /// 税码
        /// </summary>
        public string VatGroup { get; set; }
        /// <summary>
        /// 毛价
        /// </summary>
        public decimal PriceAfVAT { get; set; }
        /// <summary>
        /// 行总价
        /// </summary>
        public decimal LineTotal { get; set; }
        /// <summary>
        /// 总计(外币)
        /// </summary>
        public decimal TotalFrgn { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public decimal U_SCTCBL { get; set; }
        /// <summary>
        /// 物料成本
        /// </summary>
        public decimal StockPrice { get; set; }
        /// <summary>
        /// 运费
        /// </summary>
        public decimal U_YF { get; set; }
        /// <summary>
        /// 仓库
        /// </summary>
        public string WhsCode { get; set; }
        /// <summary>
        ///  当前库存量
        /// </summary>
        public decimal OnHand { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public decimal VatPrcnt { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int LineNum { get; set; }
        public decimal U_YFTCBL { get; set; }
        public decimal IsCommited { get; set; }
        public decimal OnOrder { get; set; }
        public int U_TDS { get; set; }
        public int U_DL { get; set; }
        public int U_DY { get; set; }
        public int DocEntry { get; set; }
        public decimal OpenQty { get; set; }
        public decimal U_JGF { get; set; }
        public int QryGroup1 { get; set; }
        public int QryGroup2 { get; set; }
        public int QryGroup3 { get; set; }
        public decimal U_YFCB { get; set; }
        public decimal U_SHJSDJ { get; set; }
        public decimal U_SHJSJ { get; set; }
        public decimal U_SHTC { get; set; }
        /// <summary>
        /// 出货数量
        /// </summary>
        public decimal SumQuantity { get; set; }
        public int QryGroup8 { get; set; }
        public int QryGroup9 { get; set; }
        public int QryGroup10 { get; set; }
        public string buyunitmsr { get; set; }
        /// <summary>
        /// 配置类型
        /// </summary>
        public string U_ZS { get; set; }
        /// <summary>
        /// 关联评审单
        /// </summary>
        public string U_RelDoc { get; set; }
        /// <summary>
        /// 状态
        /// </summary>
        public string LineStatus { get; set; }
        /// <summary>
        ///  物料编码
        /// </summary>
        public string BaseEntry { get; set; }
        public string BaseLine { get; set; }
        public int BaseType { get; set; }

    }
}
