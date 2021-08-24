using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Order.ModelDto
{
    /// <summary>
    /// 最新单据
    /// </summary>
    public class HistoricalOrder
    {
        /// <summary>
        /// 单号
        /// </summary>
        public int DocEntry { get; set; }
        /// <summary>
        /// 金额
        /// </summary>
        public decimal DocTotal { get; set; }
        /// <summary>
        /// 过账日期
        /// </summary>
        public DateTime DocDate { get; set; }
    }
}
