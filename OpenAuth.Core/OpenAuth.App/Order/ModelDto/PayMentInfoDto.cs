using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Order.ModelDto
{
    /// <summary>
    /// 付款条件
    /// </summary>
    public class PayMentInfoDto
    {
        /// <summary>
        /// 预付款日期
        /// </summary>
        public int PrepaDay { get; set; }
        /// <summary>
        /// 预付百分比
        /// </summary>
        public decimal PrepaPro { get; set; }
        /// <summary>
        /// 发货前付
        /// </summary>
        public decimal PayBefShip { get; set; }
        /// <summary>
        /// 货到付百分比
        /// </summary>
        public decimal GoodsToPro { get; set; }
        /// <summary>
        /// 货到付款天数
        /// </summary>
        public int GoodsToDay { get; set; }
    }
}
