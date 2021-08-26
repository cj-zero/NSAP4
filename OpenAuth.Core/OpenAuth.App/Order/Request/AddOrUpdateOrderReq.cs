using NSAP.Entity.Sales;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Order.Request
{
    /// <summary>
    /// 订单创建/修改
    /// </summary>
    public class AddOrderReq
    {
        /// <summary>
        /// 提交类型
        /// 0：草稿
        /// 1：提交
        /// 2：再次提交
        /// </summary>
        public OrderAtion Ations { get; set; }
        /// <summary>
        /// 销售报价单ID(创建时为0)
        /// </summary>
        public int JobId { get; set; }
        /// <summary>
        /// 是否来源销售订单
        /// </summary>
        public bool IsCopy { get; set; }
        /// <summary>
        /// 商品配置模板（1;有配置清单,0：无配置清单 ）
        /// 草稿时默认为1;
        /// </summary>
        public bool IsTemplate { get; set; }
        /// <summary>
        /// 订单
        /// </summary>
        public OrderDraft Order { get; set; }
	}

    public class OrderDraft
    {
        /// <summary>
        /// 业务员
        /// </summary>
        public string U_YWY { get; set; }
        /// <summary>
        /// 凭证类型（I & S 默认值为I，I物料、S服务)
        /// </summary>
        public string DocType { get; set; }
        /// <summary>
        /// </summary>
        public string VatGroup { get; set; }
        /// <summary>
        /// 过帐时间，由程序更新。
        /// </summary>
        public DateTime DocDate { get; set; }
        /// <summary>
        /// 起息日
        /// </summary>
        public DateTime DocDueDate { get; set; }
        /// <summary>
        /// 客户/供应商代码
        /// </summary>
        public string CardCode { get; set; }
        /// <summary>
        /// 客户/供应商名称
        /// </summary>
        public string CardName { get; set; }
        /// <summary>
        /// 收票方
        /// </summary>
        public string Address { get; set; }
        /// <summary>
        /// 运达方
        /// </summary>
        public string Address2 { get; set; }
        /// <summary>
        /// 税额总计
        /// </summary>
        public decimal VatSum { get; set; }
        /// <summary>
        /// 凭证折扣率
        /// </summary>
        public decimal DiscPrcnt { get; set; }
        /// <summary>
        /// 总计折扣
        /// </summary>
        public decimal DiscSum { get; set; }
        /// <summary>
        /// 总计折扣外币
        /// </summary>
        public decimal DiscSumFC { get; set; }
        /// <summary>
        /// 凭证货币
        /// </summary>
        public string DocCur { get; set; }
        /// <summary>
        /// 凭证汇率
        /// </summary>
        public decimal DocRate { get; set; }
        /// <summary>
        /// 凭证总计
        /// </summary>
        public decimal DocTotal { get; set; }
        /// <summary>
        /// 凭证总计外币
        /// </summary>
        public decimal DocTotalFC { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string Comments { get; set; }
        /// <summary>
        /// 审核备注
        /// </summary>
        public string Remark { get; set; }
        /// <summary>
        /// 付款条款代码
        /// </summary>
        public int GroupNum { get; set; }
        /// <summary>
        /// 销售代表
        /// </summary>
        public int SlpCode { get; set; }
        /// <summary>
        /// 交货方法
        /// </summary>
        public int TrnspCode { get; set; }
        /// <summary>
        /// 部分交货（Y代表Yes,N代表No,默认值为Y）
        /// </summary>
        public string PartSupply { get; set; }
        /// <summary>
        /// 联系人
        /// </summary>
        public string CntctCode { get; set; }
        /// <summary>
        /// 基础货币（L、S、C，默认值为C,其中L本币、C业务伙伴货币、S系统货币）
        /// </summary>
        public string CurSource { get; set; }
        /// <summary>
        /// 单据日期
        /// </summary>
        public DateTime TaxDate { get; set; }
        /// <summary>
        /// 总计费用(运费)
        /// </summary>
        public decimal TotalExpns { get; set; }
        /// <summary>
        /// 标识
        /// </summary>
        public string Indicator { get; set; }
        /// <summary>
        /// 运达方代码
        /// </summary>
        public string ShipToCode { get; set; }
        /// <summary>
        /// 付款方式
        /// </summary>
        public string PeyMethod { get; set; }
        /// <summary>
        /// 付款到
        /// </summary>
        public string PayToCode { get; set; }
        /// <summary>
        /// 业务伙伴参考编号    
        /// </summary>
        public string NumAtCard { get; set; }
        /// <summary>
        /// 经理
        /// </summary>
        public int OwnerCode { get; set; }
        /// <summary>
        /// 折扣前总计
        /// </summary>
        public string BeforeDiscSum { get; set; }
        /// <summary>
        /// 发票类别
        /// </summary>
        public string U_FPLB { get; set; }
        /// <summary>
        /// 税率
        /// </summary>
        public string U_SL { get; set; }
        /// <summary>
        /// 货到付百分比
        /// </summary>
        public string GoodsToPro { get; set; }
        /// <summary>
        /// 货到付款日期
        /// </summary>
        public string GoodsToDate { get; set; }
        /// <summary>
        /// 货到付款天数
        /// </summary>
        public string GoodsToDay { get; set; }
        /// <summary>
        /// 发货前付
        /// </summary>
        public string PayBefShip { get; set; }
        /// <summary>
        /// 预付百分比
        /// </summary>
        public string PrepaPro { get; set; }
        /// <summary>
        /// 预付款日期
        /// </summary>
        public string PrepaData { get; set; }
        /// <summary>
        /// 自定义字段
        /// </summary>
        public string CustomFields { get; set; }
        /// <summary>
        /// WhsCode
        /// </summary>
        public string WhsCode { get; set; }
        public string U_New_ORDRID { get; set; }
        /// <summary>
        /// 如果是商城订单记录商城单号
        public string U_EshopNo { get; set; }
        /// <summary>
        /// 单据关联单号
        /// </summary>
        public string U_RelDoc { get; set; }

        public string BillBaseType { get; set; }
        public string BillBaseEntry { get; set; }
        
        /// <summary>
        /// 行明细
        /// </summary>
        public IList<OrderItem> OrderItems { get; set; }
        /// <summary>
        /// 附件
        /// </summary>
        public List<billDeliveryDeatil> FileList { get; set; }
    }
    public class OrderItem
    {
        /// <summary>
        /// 物料号
        /// </summary>
        public string ItemCode { get; set; }
        /// <summary>
        /// 物料/服务描述
        /// </summary>
        public string Dscription { get; set; }
        /// <summary>
        /// 数量
        /// </summary>
        public string Quantity { get; set; }
        /// <summary>
        /// 价格
        /// </summary>
        public string Price { get; set; }
        /// <summary>
        /// 每行折扣 %
        /// </summary>
        public string DiscPrcnt { get; set; }
        /// <summary>
        /// 折扣后价格
        /// </summary>
        public string PriceBefDi { get; set; }
        /// <summary>
        /// 税定义
        /// </summary>
        public string VatGroup { get; set; }
        /// <summary>
        /// 毛价
        /// </summary>
        public string PriceAfVAT { get; set; }
        /// <summary>
        /// 行总计
        /// </summary>
        public string LineTotal { get; set; }

        /// <summary>
        /// 以外币计的行总计
        /// </summary>
        public string TotalFrgn { get; set; }
        /// <summary>
        /// 销售提成比例
        /// </summary>
        public string U_XSTCBL { get; set; }
        /// <summary>
        /// 销售提成金额
        /// </summary>
        public string U_XSTCJE { get; set; }
        /// <summary>
        /// 研发提成金额
        /// </summary>
        public string U_YFTCJE { get; set; }
        /// <summary>
        /// 生产提成金额
        /// </summary>
        public string U_SCTCJE { get; set; }
        /// <summary>
        /// 物料成本
        /// </summary>
        public string StockPrice { get; set; }
        /// <summary>
        /// 业务费
        /// </summary>
        public string U_YWF { get; set; }

        /// <summary>
        /// 服务费
        /// </summary>
        public string U_FWF { get; set; }
        /// <summary>
        /// 运费
        /// </summary>
        public string U_YF { get; set; }
        /// <summary>
        /// 仓库代码
        /// </summary>
        public string WhsCode { get; set; }
        /// <summary>
        /// 库存量
        /// </summary>
        public string OnHand { get; set; }
        /// <summary>
        /// 每行税收百分比
        /// </summary>
        public string VatPrcnt { get; set; }

        public string U_TDS { get; set; }
        public string U_DL { get; set; }
        public string U_DY { get; set; }
        /// <summary>
        /// 目标凭证类型(-1,0,13,16,203,默认值为-1)
        /// </summary>
        public string TargetType { get; set; }
        /// <summary>
        /// 目标凭证代码
        /// </summary>
        public string TrgetEntry { get; set; }

        /// <summary>
        /// 基本凭证参考
        /// </summary>
        public string BaseRef { get; set; }

        /// <summary>
        /// 基本凭证类型(-1,0,23，17，16，13，165,默认值为-1)
        /// </summary>
        public string BaseType { get; set; }

        /// <summary>
        /// 基本凭证代码
        /// </summary>
        public string BaseEntry { get; set; }

        /// <summary>
        /// 基础行
        /// </summary>
        public string BaseLine { get; set; }
        /// <summary>
        /// 配电选项
        /// </summary>
        public string U_PDXX { get; set; }
        /// <summary>
        /// 配电选项
        /// </summary>
        public string Lowest { get; set; }
        /// <summary>
        /// 配电选项
        /// </summary>
        public string ConfigLowest { get; set; }
        public string U_ZS { get; set; }
        public string IsExistMo { get; set; }
        public string ItemCfgId { get; set; }
        public string Weight { get; set; }
        public string Volume { get; set; }
        public string U_JGF { get; set; }
        public string U_JGF1 { get; set; }
        public string U_YFCB { get; set; }
        public string U_SHJSDJ { get; set; }
        public string U_SHJSJ { get; set; }
        public string U_SHTC { get; set; }
        public string QryGroup1 { get; set; }
        public string QryGroup2 { get; set; }
        public string _QryGroup3 { get; set; }
        public string QryGroup8 { get; set; }//3008n
        public string QryGroup9 { get; set; }//9系列
        public string QryGroup10 { get; set; }//ES系列
        public string U_YFTC_3008n { get; set; }//3008n
        public string SumQuantity { get; set; }//出货数量(新增)
        public string U_RelDoc { get; set; }//采购物料对应的订单情况
    }
    /// <summary>
    /// 订单操作类型
    /// </summary>
    public enum OrderAtion
    {
        /// <summary>
        /// 草稿
        /// </summary>
        Draft,
        /// <summary>
        /// 提交
        /// </summary>
        Submit,
        /// <summary>
        /// 
        /// </summary>
        Resubmit
    }
}
