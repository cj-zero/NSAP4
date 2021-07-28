﻿using System;
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