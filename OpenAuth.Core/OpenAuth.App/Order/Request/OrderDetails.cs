using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace OpenAuth.App.Order.Request
{
    /// <summary>
    /// 订单明细
    /// </summary>
    [Serializable]
    [DataContract]
    public class OrderDetails
    {/// <summary>
     /// 物料号
     /// </summary>
        [DataMember]
        public string ItemCode { get; set; }

        /// <summary>
        /// 物料/服务描述
        /// </summary>
        [DataMember]
        public string Dscription { get; set; }
        /// <summary>
        /// 数量
        /// </summary>
        [DataMember]
        public string Quantity { get; set; }

        /// <summary>
        /// 价格
        /// </summary>
        [DataMember]
        public string Price { get; set; }
        /// <summary>
        /// 每行折扣 %
        /// </summary>
        [DataMember]
        public string DiscPrcnt { get; set; }
        /// <summary>
        /// 折扣后价格
        /// </summary>
        [DataMember]
        public string PriceBefDi { get; set; }
        /// <summary>
        /// 税定义
        /// </summary>
        [DataMember]
        public string VatGroup { get; set; }
        /// <summary>
        /// 毛价
        /// </summary>
        [DataMember]
        public string PriceAfVAT { get; set; }
        /// <summary>
        /// 行总计
        /// </summary>
        [DataMember]
        public string LineTotal { get; set; }

        /// <summary>
        /// 以外币计的行总计
        /// </summary>
        [DataMember]
        public string TotalFrgn { get; set; }

        /// <summary>
        /// 销售提成比例
        /// </summary>
        [DataMember]
        public string U_XSTCBL { get; set; }
        /// <summary>
        /// 销售提成金额
        /// </summary>
        [DataMember]
        public string U_XSTCJE { get; set; }

        /// <summary>
        /// 补助金额
        /// </summary>
        [DataMember]
        public string U_BZJE { get; set; }
        /// <summary>
        /// 补助后金额
        /// </summary>
        [DataMember]
        public string U_BZHJE { get; set; }
        /// <summary>
        /// 补助后金额
        /// </summary>
        [DataMember]
        public string Remarks { get; set; }
        /// <summary>
        /// 研发提成金额
        /// </summary>
        [DataMember]
        public string U_YFTCJE { get; set; }
        /// <summary>
        /// 生产提成金额
        /// </summary>
        [DataMember]
        public string U_SCTCJE { get; set; }

        /// <summary>
        /// 物料成本
        /// </summary>
        [DataMember]
        public string StockPrice { get; set; }

        /// <summary>
        /// 业务费
        /// </summary>
        [DataMember]
        public string U_YWF { get; set; }

        /// <summary>
        /// 服务费
        /// </summary>
        [DataMember]
        public string U_FWF { get; set; }

        /// <summary>
        /// 运费
        /// </summary>
        [DataMember]
        public string U_YF { get; set; }

        /// <summary>
        /// 仓库代码
        /// </summary>
        [DataMember]
        public string WhsCode { get; set; }
        /// <summary>
        /// 库存量
        /// </summary>
        [DataMember]
        public string OnHand { get; set; }

        /// <summary>
        /// 每行税收百分比
        /// </summary>
        [DataMember]
        public string VatPrcnt { get; set; }

        [DataMember]
        public string U_TDS { get; set; }
        [DataMember]
        public string U_DL { get; set; }
        [DataMember]
        public string U_DY { get; set; }
        /// <summary>
        /// 目标凭证类型(-1,0,13,16,203,默认值为-1)
        /// </summary>
        [DataMember]
        public string TargetType { get; set; }

        /// <summary>
        /// 目标凭证代码
        /// </summary>
        [DataMember]
        public string TrgetEntry { get; set; }

        /// <summary>
        /// 基本凭证参考
        /// </summary>
        [DataMember]
        public string BaseRef { get; set; }

        /// <summary>
        /// 基本凭证类型(-1,0,23，17，16，13，165,默认值为-1)
        /// </summary>
        [DataMember]
        public string BaseType { get; set; }

        /// <summary>
        /// 基本凭证代码
        /// </summary>
        [DataMember]
        public string BaseEntry { get; set; }

        /// <summary>
        /// 基础行
        /// </summary>
        [DataMember]
        public string BaseLine { get; set; }

        /// <summary>
        /// 配电选项
        /// </summary>
        [DataMember]
        public string U_PDXX { get; set; }
        /// <summary>
        /// 配电选项
        /// </summary>
        [DataMember]
        public string Lowest { get; set; }
        /// <summary>
        /// 配电选项
        /// </summary>
        [DataMember]
        public string ConfigLowest { get; set; }
        /// <summary>
        /// 配电选项
        /// </summary>
        [DataMember]

        public string Deductible { get; set; }

        public string WattPrice { get; set; }

        public string IsExistMo { get; set; }
        // public string Remarks { get; set; }

        [DataMember]
        public string ItemCfgId { get; set; }
        [DataMember]
        public string IsCommited { get; set; }

        [DataMember]
        public string OnOrder { get; set; }
        [DataMember]
        public string OnAvailable { get; set; }
        [DataMember]
        public string Weight { get; set; }
        [DataMember]
        public string Volume { get; set; }
        [DataMember]
        public string U_JGF { get; set; }
        [DataMember]
        public string U_JGF1 { get; set; }
        [DataMember]
        public string U_YFCB { get; set; }
        [DataMember]//U_SHJSDJ  U_SHJSJ   U_SHTC
        public string U_SHJSDJ { get; set; }
        [DataMember]
        public string U_SHJSJ { get; set; }
        [DataMember]
        public string U_SHTC { get; set; }
        [DataMember]
        public string U_XSTC { get; set; }
        [DataMember]
        public string U_YFTC_CCDC { get; set; }
        [DataMember]
        public string U_YFTC_EVT { get; set; }
        [DataMember]
        public string U_YFTC_4 { get; set; }
        [DataMember]
        public string QryGroup1 { get; set; }
        [DataMember]
        public string QryGroup2 { get; set; }
        [DataMember]
        public string _QryGroup3 { get; set; }
        [DataMember]
        public string Available { get; set; }
        [DataMember]
        public string MinLevel { get; set; }
        [DataMember]
        public string PurPackUn { get; set; }
        //扣减费用
        [DataMember]
        public string U_KJFY { get; set; }
                                          //  [DataMember]
                                          //  public string U_BZJE { get; set; }//补助金额(已存在)
        [DataMember]
        public string QryGroup8 { get; set; }//3008n
        [DataMember]
        public string QryGroup9 { get; set; }//9系列
        [DataMember]
        public string QryGroup10 { get; set; }//ES系列
        [DataMember]
        public string U_YFTC_3008n { get; set; }//3008n
        [DataMember]
        public string U_YFTC_9 { get; set; }//9系列
        [DataMember]
        public string U_YFTC_ES { get; set; }//ES系列
        [DataMember]
        public string SumQuantity { get; set; }//出货数量(新增)
        [DataMember]
        public string DeliveredQuantity { get; set; }//交货数量(取代SumQuantity)

        [DataMember]
        public string U_RelDoc { get; set; }//采购物料对应的订单情况
        [DataMember]
        public string U_RelQty { get; set; }//采购物料对应的订单数量总计
        [DataMember]
        public string U_ZS { get; set; }//配置类型
        #region 提成需要用到参数
        [DataMember]
        public string U_TaxJE { get; set; }//税额
        [DataMember]
        public string U_HL_Gap { get; set; }
        [DataMember]
        public string POR_Price { get; set; }//对应采购价格
        [DataMember]
        public string U_FZTDF { get; set; }
        [DataMember]
        public string U_FDYF { get; set; }
        [DataMember]
        public string U_ZWJF { get; set; }
        [DataMember]
        public string U_JZGZF { get; set; }
        [DataMember]
        public string U_CPF { get; set; }
        [DataMember]
        public string U_TSCLF { get; set; }

        #endregion

        [DataMember]
        public IList<SerialNumberChooseItem> ChoosedSerialNumberList;

        [DataMember]
        public string LineNum { get; set; }

        [DataMember]
        public string LineStatus { get; set; }
        /// <summary>
        /// 未清数量
        /// </summary>
        [DataMember]
        public string OpenQty { get; set; }
    }
}
