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
        /// 草稿/提交
        /// </summary>
        public OrderAtion Ations { get; set; }
        /// <summary>
        /// 销售报价单ID
        /// </summary>
        public int JobId { get; set; }
        /// <summary>
        /// 是否来源销售订单
        /// </summary>
        public bool IsCopy { get; set; }
        /// <summary>
        /// 商品配置模板（1;有配置清单,0：无配置清单 ）
        /// </summary>
        public bool IsTemplate { get; set; }
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
