using System;
using System.Collections.Generic;
using System.Text;
using OpenAuth.Repository.Domain;
using OpenAuth.App.Request;

namespace OpenAuth.App.PayTerm.PayTermSetHelp
{
    public class PayAutoFreezeRule
    {
        /// <summary>
        /// 单据编号
        /// </summary>
        public int DocEntry { get; set; }

        /// <summary>
        /// 总价
        /// </summary>
        public decimal? DocTotal { get; set; }

        /// <summary>
        /// 总价（外币）
        /// </summary>
        public decimal? DocTotalFC { get; set; }

        /// <summary>
        /// 客户编码
        /// </summary>
        public string CardCode { get; set; }

        /// <summary>
        /// 客户名称
        /// </summary>
        public string CardName { get; set; }
    }

    public class RuleDLN1 : PayAutoFreezeRule
    {
        /// <summary>
        /// 基础单据编号
        /// </summary>
        public int BaseEntry { get; set; }
    }

    public class RuleORCT : PayAutoFreezeRule
    {
        /// <summary>
        /// 编码
        /// </summary>
        public int U_XSDD { get; set; }
    }

    public class RuleORDR : PayAutoFreezeRule
    { 
        /// <summary>
        /// 总计
        /// </summary>
        public decimal? SumDocTotal { get; set; } 

        /// <summary>
        /// 总计（外币）
        /// </summary>
        public decimal? SumDocTotalFC { get; set; }
    }

    public class RuleCustomer
    {
        /// <summary>
        /// 客户编码
        /// </summary>
        public string CardCode { get; set; }

        /// <summary>
        /// 客户名称
        /// </summary>
        public string CardName { get; set; }
    }

    public class CustomerReq : PageReq
    {
        /// <summary>
        /// 客户
        /// </summary>
        public string CustomerCodeOrName { get; set; }

        /// <summary>
        /// 业务员
        /// </summary>
        public string SaleUser { get; set; }
    }
}
