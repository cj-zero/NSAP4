using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.PayTerm.PayTermSetHelp
{
    public class SaleOrderFlowHelp
    {
        public SaleOrderFlowHelp()
        {
            this.Flag = false;
            this.Name = string.Empty;
            this.Dept = string.Empty;
        }

        /// <summary>
        /// 模块名称
        /// </summary>
        public string ModelName { get; set; }

        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 部门
        /// </summary>
        public string Dept { get; set; }

        /// <summary>
        /// 数据时间
        /// </summary>
        public DateTime? DataTime { get; set; }

        /// <summary>
        /// 标识
        /// </summary>
        public bool Flag { get; set; }
    }

    public class SaleOrderORCT
    {
        /// <summary>
        /// 单据编号
        /// </summary>
        public int Docentry { get; set; }

        /// <summary>
        /// 总金额
        /// </summary>
        public decimal? sum_DocTotal { get; set; }

        /// <summary>
        /// 总金额（外币）
        /// </summary>
        public decimal? sum_DocTotalFC { get; set; }
    }

    public class SaleBuyOPOR
    {
        /// <summary>
        /// 物料编码
        /// </summary>
        public string ItemCode { get; set; }

        /// <summary>
        /// 采购单号
        /// </summary>
        public int DocEntry { get; set; }

        /// <summary>
        /// 采购员
        /// </summary>
        public string U_YGMD { get; set; }

        /// <summary>
        /// 采购单创建时间
        /// </summary>
        public DateTime? CreateDate { get; set; }

        /// <summary>
        /// 交货时间
        /// </summary>
        public DateTime? DocDueDate { get; set; }

        /// <summary>
        /// 入库单号
        /// </summary>
        public string EnterWareHouseNum { get; set; }

        /// <summary>
        /// 入库状态
        /// </summary>
        public string EnterWareHouseStatus { get; set; }
    }

    public class SaleProductOWOR
    {
        /// <summary>
        /// 生产订单号
        /// </summary>
        public int DocEntry { get; set; }

        /// <summary>
        /// 物料编码
        /// </summary>
        public string ItemCode { get; set; }

        /// <summary>
        /// 计划数量
        /// </summary>
        public decimal? PlanNum { get; set; }

        /// <summary>
        /// 完成数量
        /// </summary>
        public decimal? ComNum { get; set; }

        /// <summary>
        /// 生产订单审批状态
        /// </summary>
        public string ProductStatus { get; set; }

        /// <summary>
        /// 生产订单创建时间
        /// </summary>
        public DateTime? CreateTime { get; set; }

        /// <summary>
        /// 生产部门
        /// </summary>
        public string ProductDept { get; set; }

        /// <summary>
        /// 生产收货单号
        /// </summary>
        public string ProductReceNum { get; set; }

        /// <summary>
        /// 仓库
        /// </summary>
        public string WareHouse { get; set; }
    }

    public class SaleReceORCT 
    {
        /// <summary>
        /// 单据编码
        /// </summary>
        public string ReceNum { get; set; }

        /// <summary>
        /// 销售单号
        /// </summary>
        public string SaleNum { get; set; }

        /// <summary>
        /// 合同号
        /// </summary>
        public string ContractNo { get; set; }

        /// <summary>
        /// 合同金额
        /// </summary>
        public string ContractMoney { get; set; }

        /// <summary>
        /// 已收金额
        /// </summary>
        public string ReceMoney { get; set; }

        /// <summary>
        /// 未收金额
        /// </summary>
        public string NoReceMoney { get; set; }
    }

    public class SaleReturnGoods 
    {
        /// <summary>
        /// 退货单号
        /// </summary>
        public string ReturnNum { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime? CreatTime { get; set; }

        /// <summary>
        /// 审核状态
        /// </summary>
        public string AuditStatus { get; set; }

        /// <summary>
        /// 物料编码
        /// </summary>
        public string ItemCode { get; set; }

        /// <summary>
        /// 数量
        /// </summary>
        public decimal? Number { get; set; }

        /// <summary>
        /// 单价
        /// </summary>
        public string Price { get; set; }

        /// <summary>
        /// 金额
        /// </summary>
        public string Money { get; set; }
    }
}
