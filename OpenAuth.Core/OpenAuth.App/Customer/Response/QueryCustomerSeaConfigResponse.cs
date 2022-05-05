using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Customer.Response
{
    public class QueryCustomerSeaConfigResponse
    {
        /// <summary>
        /// 放入时间
        /// </summary>
        public TimeSpan PutTime { get; set; }

        /// <summary>
        /// 通知时间
        /// </summary>
        public TimeSpan NotifyTime { get; set; }

        /// <summary>
        /// 提前通知天数
        /// </summary>
        public int NotifyDay { get; set; }

        /// <summary>
        /// 是否启用自动放入公海
        /// </summary>
        public bool Enable { get; set; }



        /// <summary>
        /// 未报价天数;
        /// </summary>
        public int RecoverNoPrice { get; set; }

        /// <summary>
        /// 未成交天数;
        /// </summary>
        public int RecoverNoOrder { get; set; }

        /// <summary>
        /// 是否启用公海回收机制
        /// </summary>
        public bool RecoverEnable { get; set; }



        /// <summary>
        /// 每天最多认领数量;
        /// </summary>
        public int ReceiveMaxLimit { get; set; }

        /// <summary>
        /// 业务员入职最大时间
        /// </summary>
        public int ReceiveJobMax { get; set; }

        /// <summary>
        /// 业务员入职最小时间
        /// </summary>
        public int ReceiveJobMin { get; set; }

        /// <summary>
        /// 是否启用公海认领分配规则
        /// </summary>
        public bool ReceiveEnable { get; set; }



        /// <summary>
        /// 领取后天数限制
        /// </summary>
        public int AutomaticDayLimit { get; set; }

        /// <summary>
        /// 领取后次数限制
        /// </summary>
        public int AutomaticLimit { get; set; }

        /// <summary>
        /// 是否启用主动掉入公海限制
        /// </summary>
        public bool AutomaticEnable { get; set; }



        /// <summary>
        /// 掉入公海后抢回限制
        /// </summary>
        public int BackDay { get; set; }

        /// <summary>
        /// 是否启用掉入公海后抢回限制
        /// </summary>
        public bool BackEnable { get; set; }
    }
}
