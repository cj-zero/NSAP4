using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Order.Request
{
    public class OrderDataCompareReq
    {
        /// <summary>
        /// 客户对比实体集合
        /// </summary>
        public List<CustomerContrast> CustomerContrasts { get; set; }

        /// <summary>
        /// 币种汇率对比实体集合
        /// </summary>
        public List<CoinContrast> CoinContrasts { get; set; }

        /// <summary>
        /// 付款方式对比
        /// </summary>
        public List<PaymentContrast> PaymentContrasts { get; set; }

        /// <summary>
        /// 物料明细信息对比
        /// </summary>
        public List<ItemCompareDetail> ItemCompareDetails { get; set; }
    }

    /// <summary>
    /// 客户对比实体
    /// </summary>
    public class CustomerContrast
    { 
        /// <summary>
        /// 单据编号
        /// </summary>
        public string DocEntry { get; set; }

        /// <summary>
        /// 客户代码
        /// </summary>
        public string CardCode { get; set;}

        /// <summary>
        /// 客户名称
        /// </summary>
        public string CardName { get; set; }
    }

    /// <summary>
    /// 币种汇率对比实体
    /// </summary>
    public class CoinContrast
    { 
        /// <summary>
        /// 单据编号
        /// </summary>
        public string DocEntry { get; set; }

        /// <summary>
        /// 币种名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 汇率
        /// </summary>
        public decimal? DocRate { get; set; }
    }

    /// <summary>
    /// 付款条件对比
    /// </summary>
    public class PaymentContrast
    { 
        /// <summary>
        /// 单据编号
        /// </summary>
        public string DocEntry { get; set; }

        /// <summary>
        /// 付款条件
        /// </summary>
        public string GroupNum { get; set; }
    }

    /// <summary>
    /// 币种分组
    /// </summary>
    public class CoinGroupByName
    { 
        /// <summary>
        /// 币种名称
        /// </summary>
        public string Name { get; set; }
    }

    /// <summary>
    /// 物料分组
    /// </summary>
    public class ItemCodeGroup
    { 
        /// <summary>
        /// 凭证编码
        /// </summary>
        public int DocEntry { get; set; }

        /// <summary>
        /// 税率
        /// </summary>
        public decimal DocRate { get; set; }

        /// <summary>
        /// 物料编码
        /// </summary>
        public string ItemCode { get; set; }

        /// <summary>
        /// 单价
        /// </summary>
        public decimal? Price { get; set; }

        /// <summary>
        /// 数量
        /// </summary>
        public decimal? Quantity { get; set; }
    }

    /// <summary>
    /// 物料分组
    /// </summary>
    public class ItemGroup
    {
        /// <summary>
        /// 物料编码
        /// </summary>
        public string ItemCode { get; set; }

        /// <summary>
        /// 总价
        /// </summary>
        public decimal? TotalAmount { get; set; }

        /// <summary>
        /// 总数量
        /// </summary>
        public decimal? TotalQuantity { get; set; }

        /// <summary>
        /// 单价
        /// </summary>
        public decimal? Price { get; set; }

        /// <summary>
        /// 税率
        /// </summary>
        public decimal DocRate { get; set; }
    }

    /// <summary>
    /// 物料对比明细
    /// </summary>
    public class ItemCompareDetail
    { 
        /// <summary>
        /// 物料编码
        /// </summary>
        public string ItemCode { get; set; }

        /// <summary>
        /// 销售报价单物料数量
        /// </summary>
        public string QuotationQuantity { get; set; }

        /// <summary>
        /// 销售订单物料数量
        /// </summary>
        public string SaleOrderQuantity { get; set; }

        /// <summary>
        /// 当前单据与销售订单物料差异数
        /// </summary>
        public string DifferenceQuantity { get; set; }

        /// <summary>
        /// 销售报价单物料单价
        /// </summary>
        public string QuotationPrice { get; set; }

        /// <summary>
        /// 销售订单物料单价
        /// </summary>
        public string SaleOrderPrice { get; set; }

        /// <summary>
        /// 当前单据与销售订单物料单价差异
        /// </summary>
        public string DifferencePrice { get; set;}

        /// <summary>
        /// 销售报价单物料总价
        /// </summary>
        public string QutationTotalAmount { get; set;}

        /// <summary>
        /// 销售订单物料总价
        /// </summary>
        public string SaleOrderTotalAmount { get; set; }

        /// <summary>
        /// 当前单据与销售订单物料总价差异
        /// </summary>
        public string DifferenceTotalAmount { get; set; }
    }

    /// <summary>
    /// 单据比较实体
    /// </summary>
    public class OrderDataDocEntryReq 
    {
        /// <summary>
        /// 当前单据凭证编码
        /// </summary>
        public int DocEntry { get; set; }

        /// <summary>
        /// 需要比较的单据凭证编码集合
        /// </summary>
        public List<int> DocEntrys { get; set; }
    }

    /// <summary>
    /// 凭证编号实体查询
    /// </summary>
    public class QueryDocEntryReq
    {
        /// <summary>
        /// 凭证编码
        /// </summary>
        public int DocEntry { get; set; }

        /// <summary>
        /// 拟取消订单编号
        /// </summary>
        public string U_New_ORDRID { get; set; }

        /// <summary>
        /// 原凭证编号
        /// </summary>
        public int BaseEntry { get; set; }

        /// <summary>
        /// 销售订单凭证编号
        /// </summary>
        public int SaleDocEntry { get; set; }
    }
}
