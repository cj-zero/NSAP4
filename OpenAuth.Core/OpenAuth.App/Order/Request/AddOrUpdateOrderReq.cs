using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Order.Request
{
    /// <summary>
    /// 订单创建/修改
    /// </summary>
    public class AddOrUpdateOrderReq
    {
        /// <summary>
        /// 提交类型
        /// </summary>
        public OrderAtion Ations { get; set; }

        public int JobId { get; set; }
        public string Copy { get; set; }
        public string IsTemplate { get; set; }
        /// <summary>
        /// 订单
        /// </summary>
        public SaleOrder Order { get; set; }
    }
    /// <summary>
    /// 订单操作类型
    /// </summary>
    public enum OrderAtion
    {
        /// <summary>
        /// 草稿
        /// </summary>
        Draft,
        /// <summary>
        /// 提交
        /// </summary>
        Submit,
        /// <summary>
        /// 
        /// </summary>
        Resubmit
    }
}
