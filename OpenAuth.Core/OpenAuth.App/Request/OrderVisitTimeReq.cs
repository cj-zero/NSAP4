using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Request
{
    public class OrderVisitTimeReq
    {
        /// <summary>
        /// 服务单Id
        /// </summary>
        public int ServerOrderId { set; get; }

        /// <summary>
        /// 预约时间
        /// </summary>
        public DateTime BookingDate { get; set; }
    }
}
