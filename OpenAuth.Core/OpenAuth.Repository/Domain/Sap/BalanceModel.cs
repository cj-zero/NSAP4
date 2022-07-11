using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.Repository.Domain
{
    /// <summary>
    /// 查询用 非数据库表格
    /// </summary>
    public class BalanceModel
    {
        public decimal? Balance { get; set; }
        public decimal? INVtotal { get; set; }
        public decimal? RINtotal { get; set; }
        public decimal? RCTBal90 { get; set; }
        public decimal? INVBal90 { get; set; }
        public decimal? RINBal90 { get; set; }
        public decimal? INVTotal90P { get; set; }
    }
}
