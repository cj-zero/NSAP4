using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Customer.Request
{
    /// <summary>
    /// 自动放入公海设置
    /// </summary>
    public class AutoPutInCustomerSeaObjectReq
    {
        /// <summary>
        /// 每天放入时间
        /// </summary>
        public DateTime PutTime { get; set; }

        /// <summary>
        /// 通知时间
        /// </summary>
        public DateTime NotifyTime { get; set; }

        /// <summary>
        /// 提前通知天数
        /// </summary>
        public int NotifyDay { get; set; }

        /// <summary>
        /// 是否启用
        /// </summary>
        public bool Enable { get; set; }
    }
}
