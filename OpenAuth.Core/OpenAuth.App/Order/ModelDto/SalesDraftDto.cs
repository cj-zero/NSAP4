using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Order
{
    /// <summary>
    /// 销售报价单
    /// </summary>
    public class SalesDraftDto
    {
        /// <summary>
        /// 行号
        /// </summary>
        public int RowNumber { get; set; }
        /// <summary>
        /// 更新日期
        /// </summary>
        public DateTime UpdateDate { get; set; }
        /// <summary>
        /// 单据编号
        /// </summary>
        public int DocEntry { get; set; }
        /// <summary>
        /// 客户代码
        /// </summary>
        public string CardCode { get; set; }
        /// <summary>
        /// 客户名称
        /// </summary>
        public string CardName { get; set; }
        /// <summary>
        /// 单据总金额
        /// </summary>
        public decimal DocTotal { get; set; }
        /// <summary>
        /// 未清金额
        /// </summary>
        public decimal OpenDocTotal { get; set; }
        /// <summary>
        /// 创建日期
        /// </summary>
        public DateTime CreateDate { get; set; }

        /// <summary>
        /// 业务员编码
        /// </summary>
        public int SlpCode { get; set; }

        /// <summary>
        /// 部门编码
        /// </summary>
        public string DeptName { get; set; }
         
        /// <summary>
        /// 销售员
        /// </summary>
        public string SlpName { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string Comments { get; set; }
        /// <summary>
        /// 状态
        /// </summary>
        public string DocStatus { get; set; }
        /// <summary>
        /// 取消状态
        /// </summary>
        public string CANCELED { get; set; }
        /// <summary>
        /// 打印 状态
        /// </summary>
        public string Printed { get; set; }
        /// <summary>
        /// 是否有附件
        /// </summary>
        public string AttachFlag { get; set; }

        /// <summary>
        /// 是否中间商
        /// </summary>
        public int Flag { get; set; }
        /// <summary>
        /// 终端
        /// </summary>
        public string Terminals { get; set; }
    }

    /// <summary>
    /// 订单
    /// </summary>
    public class SaleOrderDeptDto
    {
        public DateTime? UpdateDate { get; set; }

        public int DocEntry { get; set; }

        public string CardCode { get; set; }

        public string CardName { get; set; }

        public decimal? DocTotal { get; set; }

        public decimal? OpenDocTotal { get; set; }

        public DateTime? CreateDate { get; set; }

        public int SlpCode { get; set; }

        public string DeptName { get; set; }

        public string Comments { get; set; }

        public string DocStatus { get; set; }

        public string Printed { get; set; }

        public string SlpName { get; set; }

        public string CANCELED { get; set; }

        public string Indicator { get; set; }

        public DateTime? DocDueDate { get; set; }

        public string PymntGroup { get; set; }

        public string billID { get; set; }

        public string ActualDocDueDate { get; set; }

        public string PrintNo { get; set; }

        public string PrintNumIndex { get; set; }

        public string billStatus { get; set; }

        public string bonusStatus { get; set; }

        public string proStatus { get; set; }

        public string IndicatorName { get; set; }

        public string EmpAcctWarn { get; set; }

        public string AttachFlag { get; set; }

        public decimal? U_DocRCTAmount { get; set; }

        public string TransFee { get; set; }

        public string DocCur { get; set; }
    }

    /// <summary>
    /// 订单详情
    /// </summary>
    public class SaleOrderDetail
    {
        public string Column1 { get; set; }

        /// <summary>
        /// 修改时间
        /// </summary>
        public DateTime? UpdateDate { get; set; }

        /// <summary>
        /// 单据编号
        /// </summary>
        public int DocEntry { get; set; }

        /// <summary>
        /// 客户编码
        /// </summary>
        public string CardCode { get; set; }

        /// <summary>
        /// 客户名称
        /// </summary>
        public string CardName { get; set; }

        /// <summary>
        /// 物料编码
        /// </summary>
        public string ItemCode { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        public string Dscription { get; set; }

        /// <summary>
        /// 数量
        /// </summary>
        public decimal? Quantity { get; set; }

        /// <summary>
        /// 单价
        /// </summary>
        public string Price { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string LineTotal { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string DocTotal { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string OpenDocTotal { get; set; }

        /// <summary>
        /// 创建日期
        /// </summary>
        public DateTime? CreateDate { get; set; }

        /// <summary>
        /// 编码
        /// </summary>
        public int SlpCode { get; set; }

        /// <summary>
        /// 部门
        /// </summary>
        public string DeptName { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string Comments { get; set; }

        /// <summary>
        /// 状态
        /// </summary>
        public string DocStatus { get; set; }

        /// <summary>
        /// 打印
        /// </summary>
        public string Printed { get; set; }

        /// <summary>
        /// 业务员
        /// </summary>
        public string SlpName { get; set; }

        /// <summary>
        /// 取消
        /// </summary>
        public string CANCELED { get; set; }

        /// <summary>
        /// 标识
        /// </summary>
        public string Indicator { get; set; }

        /// <summary>
        /// 日期
        /// </summary>
        public DateTime? DocDueDate { get; set; }

        /// <summary>
        /// 数量
        /// </summary>
        public decimal? eQuantity { get; set; }

        /// <summary>
        /// 系统操作者
        /// </summary>
        public string U_YGMD { get; set; }

        /// <summary>
        /// 编号
        /// </summary>
        public int LineNum { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string U_RelDoc { get; set; }
    }

    public class SaleMyCreates
    {
        public string Column1 { get; set; }

        public int job_id { get; set; }

        public string job_type_nm { get; set; }

        public string job_nm { get; set; }

        public string DeptName { get; set; }

        public int user_id { get; set; }

        public string user_nm { get; set; }

        public int job_state { get; set; }

        public System.DateTime? upd_dt { get; set; }

        public string remarks { get; set; }

        public int job_type_id { get; set; }

        public string card_code { get; set; }

        public string CardName { get; set; }

        public string DocTotal { get; set; }

        public int base_type { get; set; }

        public int base_entry { get; set; }

        public string step_nm { get; set; }

        public int sbo_id { get; set; }

        public string page_url { get; set; }

        public int sbo_itf_return { get; set; }

        public string sbo_nm { get; set; }

        public int sync_start { get; set; }

        public int sync_stat { get; set; }

        public int sync_sap { get; set; }
    }

    public class SaleDeptSubToMe 
    {
        public int RowNum { get; set; }

        public string Column1 { get; set; }

        public int job_id { get; set; }

        public string job_type_nm { get; set; }

        public string job_nm { get; set; }

        public string DeptName { get; set; }

        public int user_id { get; set; }

        public string user_nm { get; set; }

        public int job_state { get; set; }

        public System.DateTime? upd_dt { get; set; }

        public string remarks { get; set; }

        public int job_type_id { get; set; }

        public int step_id { get; set; }

        public string card_code { get; set; }

        public string CardName { get; set; }

        public string DocTotal { get; set; }

        public int base_type { get; set; }

        public int base_entry { get; set; }

        public string step_nm { get; set; }

        public int sbo_id { get; set; }

        public string page_url { get; set; }

        public int audit_level { get; set; }

        public string sbo_nm { get; set; }
    }

    public class SboInfoDto
    {
        /// <summary>
        /// Id
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }
    }
    /// <summary>
    /// 伙伴
    /// </summary>
    public class CardCodeDto
    {
        /// <summary>
        /// 行号
        /// </summary>
        public int RowNumber { get; set; }
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
        public string CntctPrsn { get; set; }
        /// <summary>
        /// 销售员
        /// </summary>
        public string SlpName { get; set; }
        /// <summary>
        /// 货币
        /// </summary>
        public string Currency { get; set; }
        /// <summary>
        /// 科目余额
        /// </summary>
        public decimal Balance { get; set; }
        /// <summary>
        /// 收货地址
        /// </summary>
        public string Address { get; set; }
        /// <summary>
        /// 开票地址
        /// </summary>
        public string Address2 { get; set; }
        /// <summary>
        /// 发票类型
        /// </summary>
        public string U_FPLB { get; set; }
        /// <summary>
        /// 销售员代码
        /// </summary>
        public object SlpCode { get; set; }
    }

    public class CardCountDto
    {
        public int count { get; set; }
    }
    /// <summary>
    /// 业务经理对象
    /// </summary>
    public class ManagerDto
    {
        /// <summary>
        /// Id
        /// </summary>
        public int EmpId { get; set; }
        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }
    }
    public class SaleItemDto
    {
        /// <summary>
        /// 物料编码
        /// </summary>
        public string ItemCode { get; set; }
        /// <summary>
        /// 物料描述
        /// </summary>
        public string ItemName { get; set; }
        /// <summary>
        /// 零售价
        /// </summary>
        public decimal High_Price { get; set; }
        /// <summary>
        /// 批发价
        /// </summary>
        public decimal Low_Price { get; set; }
        /// <summary>
        /// 当前库存
        /// </summary>
        public decimal OnHand { get; set; }
        /// <summary>
        /// 总库存量
        /// </summary>
        public decimal SumOnHand { get; set; }
        /// <summary>
        /// 已订购
        /// </summary>
        public decimal IsCommited { get; set; }
        /// <summary>
        /// 已承诺
        /// </summary>
        public decimal OnOrder { get; set; }
        /// <summary>
        /// 当前可用量
        /// </summary>
        public decimal OnAvailable { get; set; }
        /// <summary>
        /// 总可用量
        /// </summary>
        public decimal Available { get; set; }
        /// <summary>
        /// 配置描述:为空显示为非标配物料
        /// </summary>
        public string Item_Desp { get; set; }
        /// <summary>
        ///仓库
        /// </summary>
        public string WhsCode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public decimal Factor_1 { get; set; }
        /// <summary>
        /// 数量
        /// </summary>
        public decimal Factor_2 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public decimal Factor_3 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string U_TDS { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string U_DL { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string U_DY { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public decimal U_JGF { get; set; }
        /// <summary>
        /// 物料成本
        /// </summary>
        public decimal LastPurPrc { get; set; }
        /// <summary>
        /// 物料配置
        /// </summary>
        public int item_cfg_id { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public double QryGroup { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public double QryGroup2 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string QryGroup3 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public double QryGroup1 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int U_US { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public decimal U_FS { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public decimal SVolume { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public decimal SWeight1 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public decimal U_JGF1 { get; set; }
        /// <summary>
        /// 运费成本
        /// </summary>
        public decimal U_YFCB { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public decimal MinLevel { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public decimal PurPackUn { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public decimal item_counts { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string buyunitmsr { get; set; }

        /// <summary>
        /// 物料子集
        /// </summary>
        public List<SaleItemDtoChild> Children { get; set; }

    }

    public class SaleItemDtoChild
    {
        /// <summary>
        /// 物料编码
        /// </summary>
        public string ItemCode { get; set; }
        /// <summary>
        /// 物料描述
        /// </summary>
        public string ItemName { get; set; }
        /// <summary>
        /// 零售价
        /// </summary>
        public decimal High_Price { get; set; }
        /// <summary>
        /// 批发价
        /// </summary>
        public decimal Low_Price { get; set; }
        /// <summary>
        /// 当前库存
        /// </summary>
        public decimal OnHand { get; set; }
        /// <summary>
        /// 总库存量
        /// </summary>
        public decimal SumOnHand { get; set; }
        /// <summary>
        /// 已订购
        /// </summary>
        public decimal IsCommited { get; set; }
        /// <summary>
        /// 已承诺
        /// </summary>
        public decimal OnOrder { get; set; }
        /// <summary>
        /// 当前可用量
        /// </summary>
        public decimal OnAvailable { get; set; }
        /// <summary>
        /// 总可用量
        /// </summary>
        public decimal Available { get; set; }
        /// <summary>
        /// 配置描述:为空显示为非标配物料
        /// </summary>
        public string Item_Desp { get; set; }
        /// <summary>
        ///仓库
        /// </summary>
        public string WhsCode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public decimal Factor_1 { get; set; }
        /// <summary>
        /// 数量
        /// </summary>
        public decimal Factor_2 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public decimal Factor_3 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string U_TDS { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string U_DL { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string U_DY { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public decimal U_JGF { get; set; }
        /// <summary>
        /// 物料成本
        /// </summary>
        public decimal LastPurPrc { get; set; }
        /// <summary>
        /// 物料配置
        /// </summary>
        public int item_cfg_id { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public double QryGroup { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public double QryGroup2 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string QryGroup3 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public double QryGroup1 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int U_US { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public decimal U_FS { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public decimal SVolume { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public decimal SWeight1 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public decimal U_JGF1 { get; set; }
        /// <summary>
        /// 运费成本
        /// </summary>
        public decimal U_YFCB { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public decimal MinLevel { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public decimal PurPackUn { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public decimal item_counts { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string buyunitmsr { get; set; }
    }
    /// <summary>
    /// 
    /// </summary>
    public class RelORDRRDto
    {
        /// <summary>
        /// 订单号
        /// </summary>
        public object Docentry { get; set; }
        /// <summary>
        /// 客户代码
        /// </summary>
        public string CardCode { get; set; }
        /// <summary>
        /// 单据总金额
        /// </summary>
        public decimal DocTotal { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateDate { get; set; }
        /// <summary>
        /// 状态
        /// </summary>
        public string DocStatus { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string Comments { get; set; }
        /// <summary>
        /// 打印
        /// </summary>
        public string Printed { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string CANCELED { get; set; }
    }
}
