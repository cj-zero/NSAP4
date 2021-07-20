using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Request
{
    public class QuerySalesQuotationReq : PageReq
    {
        /// <summary>
        /// 排序字段
        /// </summary>
        public string SortName { get; set; }
        /// <summary>
        /// desc 降序，asc 升序
        /// </summary>
        public string SortOrder { get; set; }
        /// <summary>
        /// 账套Id
        /// </summary>
        public int Sboid { get; set; }
        //Sbo_id:1`a.DocEntry:1`a.CardCode:2`DocStatus:ON`a.Comments:3`c.SlpName:4
        /// <summary>
        /// 单号
        /// </summary>
        public string DocEntry { get; set; }
        /// <summary>
        /// 客户
        /// </summary>
        public string CardCode { get; set; }
        /// <summary>
        /// 状态
        /// </summary>
        public string DocStatus { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string Comments { get; set; }
        /// <summary>
        /// 销售员
        /// </summary>
        public string SlpName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Indicator { get; set; }
    }
    /// <summary>
    /// 
    /// </summary>
    public class CardCodeRequest : PageReq
    {
        /// <summary>
        /// 排序字段 客户代码：a.cardcode,客户名称:a.cardname,销售员员：b.slpname,货币：a.currency，金额：a.balance
        /// </summary>
        public string SortName { get; set; }
        /// <summary>
        /// desc 降序，asc 升序
        /// </summary>
        public string SortOrder { get; set; }
        /// <summary>
        /// 客户代码
        /// </summary>
        public string CardCode { get; set; }
    }
}
