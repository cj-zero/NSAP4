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
        public int Id { get; set; }
        /// <summary>
        /// 类型
        /// </summary>
        public string FileType { get; set; }
        /// <summary>
        /// 附件名称
        /// </summary>
        public string FileName { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string Remarks { get; set; }
        /// <summary>
        /// 路径
        /// </summary>
        public string FilePath { get; set; }
        /// <summary>
        /// 上传时间
        /// </summary>
        public DateTime CreateTime { get; set; }
        /// <summary>
        /// 操作者
        /// </summary>
        public string CreateUserName { get; set; }
        /// <summary>
        /// 浏览地址
        /// </summary>
        public string ViewFilePath { get; set; }
        /// <summary>
        /// 文件来源
        /// </summary>
        public string FileSource { get; set; }

    }

    /// <summary>
    /// 物料
    /// </summary>
    public class OrderItemInfo
    {
        /// <summary>
        /// 子集
        /// </summary>
        public List<OrderItemInfo> childBillSalesDetails { get; set; }

        /// <summary>
        /// 物料等级
        /// </summary>
        public string Level { get; set; }

        /// <summary>
        /// 物料配置Id
        /// </summary>
        public int item_cfg_id { get; set; }

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
    /// <summary>
    /// 查看呼叫服务信息
    /// </summary>
    public class GetCustomerInfoDto
    {
        /// <summary>
        /// 编号
        /// </summary>
        public string CardCode { get; set; }
        /// <summary>
        /// 名称
        /// </summary>
        public string CardName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string CardFName { get; set; }
        public string CmpPrivate { get; set; }
        /// <summary>
        /// 电话1
        /// </summary>
        public string Phone1 { get; set; }
        /// <summary>
        /// 电话2
        /// </summary>
        public string Phone2 { get; set; }
        /// <summary>
        /// 传真
        /// </summary>
        public string Fax { get; set; }

        public string Cellular { get; set; }
        /// <summary>
        /// 业务员
        /// </summary>
        public string SlpName { get; set; }
        public string CntctPrsn { get; set; }
        public string Notes { get; set; }
        /// <summary>
        /// 科目余额
        /// </summary>
        public decimal Balance { get; set; }
        public string Industry { get; set; }
        public string Business { get; set; }
        public string ShipType { get; set; }
        public string Address { get; set; }
        public string Building { get; set; }
        public string validFrom { get; set; }
        public string validTo { get; set; }
        /// <summary>
        /// 技术员
        /// </summary>
        public string tcnician { get; set; }
        public string MailCounty { get; set; }
        public string VatldUnCmp { get; set; }
        /// <summary>
        /// 邮箱
        /// </summary>
        public string E_Mail { get; set; }
        public int GroupNum { get; set; }
        public string Currency { get; set; }
        public string GTSRegNum { get; set; }
        public string GTSBankAct { get; set; }
        public string GTSBilAddr { get; set; }
        public string U_PYSX { get; set; }
        public string U_Name { get; set; }
        public string U_FName { get; set; }
        /// <summary>
        /// 发票类别
        /// </summary>
        public string U_FPLB { get; set; }
        public int U_job_id { get; set; }
    }
    /// <summary>
    /// 查询单个物料信息
    /// </summary>
    public class SelectSingleStoreOitmInfoDto
    {
        /// <summary>
        /// 物料编码
        /// </summary>
        public string ItemCode { get; set; }
        /// <summary>
        /// 名称
        /// </summary>
        public string ItemName { get; set; }
        /// <summary>
        /// 外文名称
        /// </summary>
        public string FrgnName { get; set; }
        /// <summary>
        /// 物料组
        /// </summary>
        public int ItmsGrpCod { get; set; }
        /// <summary>
        /// 关税组
        /// </summary>
        public int CstGrpCode { get; set; }
        /// <summary>
        /// 税收组
        /// </summary>
        public string VatGourpSa { get; set; }
        /// <summary>
        /// 条形码
        /// </summary>
        public string BarCode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string VATLiable { get; set; }
        /// <summary>
        /// 采购物料
        /// </summary>
        public string PrchseItem { get; set; }
        /// <summary>
        /// 销售物料
        /// </summary>
        public string SellItem { get; set; }
        /// <summary>
        /// 仓库物料
        /// </summary>
        public string InvntItem { get; set; }
        /// <summary>
        /// 存货量
        /// </summary>
        public decimal OnHand { get; set; }
        /// <summary>
        /// 已承诺
        /// </summary>
        public decimal IsCommited { get; set; }
        /// <summary>
        /// 已订购
        /// </summary>
        public decimal OnOrder { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string IncomeAcct { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string ExmptIncom { get; set; }
        /// <summary>
        /// 最大库存
        /// </summary>
        public decimal MaxLevel { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string DfltWH { get; set; }
        /// <summary>
        /// 首选供应商
        /// </summary>
        public string CardCode { get; set; }
        /// <summary>
        /// 制造商目录编号
        /// </summary>
        public string SuppCatNum { get; set; }
        /// <summary>
        /// 采购计量单位
        /// </summary>
        public string BuyUnitMsr { get; set; }
        /// <summary>
        /// 每采购单位数量
        /// </summary>
        public decimal NumInBuy { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public decimal ReorderQty { get; set; }
        /// <summary>
        /// 最小库存
        /// </summary>
        public decimal MinLevel { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public decimal LstEvlPric { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string LstEvlDate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public decimal CustomPer { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Canceled { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string MnufctTime { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string WholSlsTax { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string RetilrTax { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public decimal SpcialDisc { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string DscountCod { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string TrackSales { get; set; }
        /// <summary>
        /// 销售计量单位
        /// </summary>
        public string SalUnitMsr { get; set; }
        /// <summary>
        /// 每销售单位数量
        /// </summary>
        public decimal NumInSale { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public decimal Consig { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int QueryGroup { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public decimal Counted { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public decimal OpenBlnc { get; set; }
        /// <summary>
        /// 评估方法
        /// </summary>
        public string EvalSystem { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int UserSign { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string FREE { get; set; }
        /// <summary>
        /// 图片名称
        /// </summary>
        public string PicturName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Transfered { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string BlncTrnsfr { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string UserText { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string SerialNum { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public decimal CommisPcnt { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public decimal CommisSum { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int CommisGrp { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string TreeType { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public decimal TreeQty { get; set; }
        /// <summary>
        /// 最近采购价
        /// </summary>
        public decimal LastPurPrc { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string LastPurCur { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public DateTime LastPurDat { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string ExitCur { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public decimal ExitPrice { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string ExitWH { get; set; }
        /// <summary>
        /// 固定资产
        /// </summary>
        public string AssetItem { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string WasCounted { get; set; }

        public string ManSerNum { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string SHeight1 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string SHght1Unit { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string SHeight2 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string SHght2Unit { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string SWidth1 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string SWdth1Unit { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string SWidth2 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string SWdth2Unit { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string SLength1 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string SLen1Unit { get; set; }
        public string Slength2 { get; set; }
        public string SLen2Unit { get; set; }
        public decimal SVolume { get; set; }
        public int SVolUnit { get; set; }
        public string SWeight1 { get; set; }
        public string SWght1Unit { get; set; }
        public string SWeight2 { get; set; }
        public string SWght2Unit { get; set; }
        public string BHeight1 { get; set; }
        public string BHght1Unit { get; set; }
        public string BHeight2 { get; set; }
        public string BHght2Unit { get; set; }
        public string BWidth1 { get; set; }
        public string BWdth1Unit { get; set; }
        public string BWidth2 { get; set; }
        public string BWdth2Unit { get; set; }
        public string BLength1 { get; set; }
        public string BLen1Unit { get; set; }
        public string Blength2 { get; set; }
        public string BLen2Unit { get; set; }
        public decimal BVolume { get; set; }
        public int BVolUnit { get; set; }
        public string BWeight1 { get; set; }
        public string BWght1Unit { get; set; }
        public string BWeight2 { get; set; }
        public string BWght2Unit { get; set; }
        public string FixCurrCms { get; set; }
        public int FirmCode { get; set; }
        public string LstSalDate { get; set; }
        public string QryGroup1 { get; set; }
        public string QryGroup2 { get; set; }
        public string QryGroup3 { get; set; }
        public string QryGroup4 { get; set; }
        public string QryGroup5 { get; set; }
        public string QryGroup6 { get; set; }
        public string QryGroup7 { get; set; }
        public string QryGroup8 { get; set; }
        public string QryGroup9 { get; set; }
        public string QryGroup10 { get; set; }
        public string QryGroup11 { get; set; }
        public string QryGroup12 { get; set; }
        public string QryGroup13 { get; set; }
        public string QryGroup14 { get; set; }
        public string QryGroup15 { get; set; }
        public string QryGroup16 { get; set; }
        public string QryGroup17 { get; set; }
        public string QryGroup18 { get; set; }
        public string QryGroup19 { get; set; }
        public string QryGroup20 { get; set; }
        public string QryGroup21 { get; set; }
        public string QryGroup22 { get; set; }
        public string QryGroup23 { get; set; }
        public string QryGroup24 { get; set; }
        public string QryGroup25 { get; set; }
        public string QryGroup26 { get; set; }
        public string QryGroup27 { get; set; }
        public string QryGroup28 { get; set; }
        public string QryGroup29 { get; set; }
        public string QryGroup30 { get; set; }
        public string QryGroup31 { get; set; }
        public string QryGroup32 { get; set; }
        public string QryGroup33 { get; set; }
        public string QryGroup34 { get; set; }
        public string QryGroup36 { get; set; }
        public string QryGroup37 { get; set; }
        public string QryGroup38 { get; set; }
        public string QryGroup39 { get; set; }
        public string QryGroup40 { get; set; }
        public string QryGroup41 { get; set; }
        public string QryGroup42 { get; set; }
        public string QryGroup43 { get; set; }
        public string QryGroup44 { get; set; }
        public string QryGroup45 { get; set; }
        public string QryGroup46 { get; set; }
        public string QryGroup47 { get; set; }
        public string QryGroup48 { get; set; }
        public string QryGroup49 { get; set; }
        public string QryGroup50 { get; set; }
        public string QryGroup51 { get; set; }
        public string QryGroup52 { get; set; }
        public string QryGroup53 { get; set; }
        public string QryGroup54 { get; set; }
        public string QryGroup55 { get; set; }
        public string QryGroup56 { get; set; }
        public string QryGroup57 { get; set; }
        public string QryGroup58 { get; set; }
        public string QryGroup59 { get; set; }
        public string QryGroup60 { get; set; }
        public string QryGroup61 { get; set; }
        public string QryGroup62 { get; set; }
        public string QryGroup63 { get; set; }
        public string QryGroup64 { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime UpdateDate { get; set; }
        public string ExportCode { get; set; }
        /// <summary>
        /// 因子1
        /// </summary>
        public decimal SalFactor1 { get; set; }
        /// <summary>
        /// 因子2
        /// </summary>
        public decimal SalFactor2 { get; set; }
        /// <summary>
        /// 因子3
        /// </summary>
        public decimal SalFactor3 { get; set; }
        /// <summary>
        /// 因子4
        /// </summary>
        public decimal SalFactor4 { get; set; }
        public decimal PurFactor1 { get; set; }
        /// <summary>
        /// 因子2
        /// </summary>
        public decimal PurFactor2 { get; set; }
        /// <summary>
        ///因子3
        /// </summary>
        public decimal PurFactor3 { get; set; }
        /// <summary>
        /// 因子4
        /// </summary>
        public decimal PurFactor4 { get; set; }
        public string SalFormula { get; set; }
        public string PurFormula { get; set; }
        /// <summary>
        /// 税收组
        /// </summary>
        public string VatGroupPu { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public decimal AvgPrice { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string PurPackMsr { get; set; }
        public decimal PurPackUn { get; set; }
        /// <summary>
        /// 包装计量单位
        /// </summary>
        public string SalPackMsr { get; set; }
        /// <summary>
        /// 每包装单位数量
        /// </summary>
        public decimal SalPackUn { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string ManBtchNum { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string ManOutOnly { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string ValidFor { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string ValidFrom { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string ValidTo { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string FrozenFor { get; set; }
        public string FrozenFrom { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string FrozenTo { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string BlockOut { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string ValidComm { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string FrozenComm { get; set; }
        public int ObjType { get; set; }
        public string SWW { get; set; }
        public string Deleted { get; set; }
        /// <summary>
        /// 订单号
        /// </summary>
        public int DocEntry { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string ExpensAcct { get; set; }
        public string FrgnInAcct { get; set; }
        /// <summary>
        /// 装运类型
        /// </summary>
        public string ShipType { get; set; }
        /// <summary>
        /// 设置总账科目
        /// </summary>
        public string GLMethod { get; set; }
        public string ECInAcct { get; set; }
        public string FrgnExpAcc { get; set; }
        public string ECExpAcc { get; set; }
        public string TaxType { get; set; }
        /// <summary>
        /// 仓库个别管理
        /// </summary>
        public string ByWh { get; set; }
        public string WTLiable { get; set; }
        public string ItemType { get; set; }
        public string WarrntTmpl { get; set; }
        public string BaseUnit { get; set; }
        public decimal StockValue { get; set; }
        /// <summary>
        /// 虚拟物料
        /// </summary>
        public string Phantom { get; set; }
        /// <summary>
        /// 发货方式
        /// </summary>
        public string IssueMthd { get; set; }
        public string FREE1 { get; set; }
        public decimal PricingPrc { get; set; }
        /// <summary>
        /// 物料管理
        /// </summary>
        public string MngMethod { get; set; }
        public decimal ReorderPnt { get; set; }
        /// <summary>
        /// 库存计量单位
        /// </summary>
        public string InvntryUom { get; set; }
        public string PlaningSys { get; set; }
        public string PrcrmntMtd { get; set; }
        public string OrdrIntrvl { get; set; }
        public decimal OrdrMulti { get; set; }
        public decimal MinOrdrQty { get; set; }
        public string LeadTime { get; set; }
        public string IndirctTax { get; set; }
        public string TaxCodeAR { get; set; }
        public string TaxCodeAP { get; set; }
        public int ServiceGrp { get; set; }
        public int MatType { get; set; }
        public int MatGrp { get; set; }
        public string ProductSrc { get; set; }
        public int ServiceCtg { get; set; }
        public int ItemClass { get; set; }
        public string Excisable { get; set; }
        public int ChapterID { get; set; }
        public string NotifyASN { get; set; }
        public string ProAssNum { get; set; }
        public decimal AssblValue { get; set; }
        /// <summary>
        /// 物料规格说明
        /// </summary>
        public string Spec { get; set; }
        /// <summary>
        /// 物料商品税目
        /// </summary>
        public string TaxCtg { get; set; }
        public int Series { get; set; }
        public string Number { get; set; }
        public string ToleranDay { get; set; }
        public int ItemCodeType { get; set; }
        /// <summary>
        /// 采购员
        /// </summary>
        public string SlpName { get; set; }
        public string U_ItemCode { get; set; }
        public decimal U_JGF1 { get; set; }
        public int U_FS { get; set; }
        public int U_US { get; set; }
        public string U_YFCB { get; set; }
        public decimal U_U_GS { get; set; }
        public decimal U_GS_DJ { get; set; }
        public decimal U_JGF { get; set; }
        public int U_JGZQ { get; set; }
        public string U_FDY { get; set; }
    }
    /// <summary>
    /// 获取物料类型的示例
    /// </summary>
    public class GetItemTypeExpInfoDto
    {
        public int type_id { get; set; }
        public int code_rule { get; set; }
        public string type_coding_exp { get; set; }
        public string type_desc_exp { get; set; }
    }
    /// <summary>
    /// 获取物料类型的自定义字段
    /// </summary>
    public class GetItemTypeCustomFieldsDto
    {
        public int TypeID { get; set; }
        public string Fld_nm { get; set; }
        public int Fld_Alias { get; set; }
        public string Fld_Desc { get; set; }
        public int EditType { get; set; }
        public int EditSizeMin { get; set; }
        public int EditSizeMax { get; set; }
        public string Fld_dflt { get; set; }
        public string NotNull { get; set; }
        public int valRows { get; set; }

    }

    public class GetItemTypeCustomValueDto
    {
        public int SlpCode { get; set; }
        public string SlpName { get; set; }

    }
    /// <summary>
    /// 货币下拉
    /// </summary>
    public class DropPopupDocCurDto
    {
        public string id { get; set; }
        public string name { get; set; }

    }
    /// <summary>
    /// 单位
    /// </summary>
    public class DropListUnit
    {
        public int id { get; set; }
        public string name { get; set; }
    }
    public class CurrencyList
    {
        public int id { get; set; }
        public string name { get; set; }
    }
    public class CustomFieldsNewDto
    {
        public string AliasID { get; set; }
        public string U_ShipName { get; set; }
        public string Descr { get; set; }
        public string FieldID { get; set; }
        public string TableID { get; set; }
        public string NewEditType { get; set; }
        public string EditSize { get; set; }
        public List<LineDto> Line { get; set; }
    }
    public class LineDto
    {
        public string id { get; set; }
        public string name { get; set; }
    }
    //暂时注释
    //public class GetMaterialsPurHistoryDto {
    //	public string  sbo_nm { get; set; }
    //	public string DocEntry { get; set; }
    //	public string ObjType { get; set; }
    //	public string SlpName { get; set; }
    //	public DateTime DocDate { get; set; }
    //	public DateTime DocDueDate { get; set; }
    //	public string Dscription { get; set; }
    //	public string Quantity { get; set; }
    //	public string Price { get; set; }
    //	public string LineTotal { get; set; }
    //	public string CardName { get; set; }
    //	public string Comments { get; set; }
    //	public string sbo_id { get; set; }
    //	public string DocStatus { get; set; }
    //	public string Prinred { get; set; }
    //	public string CANCELED { get; set; }
    //	public string pdn_no { get; set; }
    //	public string pdn_quantity { get; set; }
    //}

    public class CopyItemMsg
    {
        public string baseLine { get; set; }

        public string buyunitmsr { get; set; }

        public string cardCode { get; set; }

        public string cardName { get; set; }

        public int docEntry { get; set; }

        public string dscription { get; set; }

        public decimal? U_SHJSDJ { get; set; }

        public decimal? U_SHJSJ { get; set; }

        public decimal? U_SHTC { get; set; }

        public string U_YFCB { get; set; }

        public int isCommited { get; set; }

        public string itemCode { get; set; }

        public decimal? lastPurPrc { get; set; }

        public decimal? lineTotal { get; set; }

        public decimal? minLevel { get; set; }

        public decimal? onAvailable { get; set; }

        public decimal? onHand { get; set; }

        public decimal? onHandS { get; set; }

        public decimal? onOrder { get; set; }

        public decimal? price { get; set; }

        public int purPackUn { get; set; }

        public int qryGroup { get; set; }

        public string qryGroup3 { get; set; }

        public decimal? quantity { get; set; }

        public int rowNum { get; set; }

        public int sVolume { get; set; }

        public int sWeight1 { get; set; }

        public string u_DL { get; set; }

        public string u_DY { get; set; }

        public decimal u_FS { get; set; }

        public decimal? u_JGF { get; set; }

        public decimal? u_JGF1 { get; set; }

        public string u_PDXX { get; set; }

        public string u_TDS { get; set; }

        public decimal u_US { get; set; }

        public string whsCode { get; set; }

        public int item_cfg_id { get; set; }

        public List<CopyItemMsg> childBillSalesDetails { get; set; }
    }
}
