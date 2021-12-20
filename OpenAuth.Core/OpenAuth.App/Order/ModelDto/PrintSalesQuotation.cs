﻿using OpenAuth.Repository.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Order.ModelDto
{
    /// <summary>
    /// 销售报价单打印
    /// </summary>
    public class PrintSalesQuotation
    {
        /// <summary>
        /// 销售报价单单号
        /// </summary>
        public string DocEntry { get; set; }
        /// <summary>
        /// 日期
        /// </summary>
        public string DateTime { get; set; }
        /// <summary>
        /// 销售
        /// </summary>
        public string SalseName { get; set; }
        /// <summary>
        /// 客户编号
        /// </summary>
        public string CardCode { get; set; }
        /// <summary>
        /// 联系人
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 电话
        /// </summary>
        public string Tel { get; set; }
        /// <summary>
        /// 传真
        /// </summary>
        public string Fax { get; set; }
        /// <summary>
        /// 客户名称
        /// </summary>
        public string CardName { get; set; }
        /// <summary>
        /// 客户地址
        /// </summary>
        public string Address { get; set; }
        /// <summary>
        /// 供方联系人电话
        /// </summary>
        public string Cellolar { get; set; }
        /// <summary>
        /// 供方地址
        /// </summary>
        public string Address2 { get; set; }
        /// <summary>
        /// 付款条件
        /// </summary>
        public string PymntGroup { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string Comments { get; set; }
        /// <summary>
        /// 金额
        /// </summary>
        public string DocTotal { get; set; }
        /// <summary>
        /// logo
        /// </summary>
        public string logo { get; set; }

        /// <summary>
        /// 二维码
        /// </summary>
        public string QRcode { get; set; }
        /// <summary>
        /// 交货日期
        /// </summary>
        public string DATEFORMAT { get; set; }
        /// <summary>
        /// 物料信息
        /// </summary>
        public List<ReimburseCost> ReimburseCosts { get; set; }


    }

    /// <summary>
    /// 物料信息
    /// </summary>
    public class ReimburseCost 
    {
        /// <summary>
        /// 产品编码
        /// </summary>
        public string ItemCode { get; set; }
        /// <summary>
        /// 描述
        /// </summary>
        public string Dscription { get; set; }
        /// <summary>
        /// 数量
        /// </summary>
        public string Quantity { get; set; }
        /// <summary>
        /// 单位
        /// </summary>
        public string unitMsr{ get; set; }
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
