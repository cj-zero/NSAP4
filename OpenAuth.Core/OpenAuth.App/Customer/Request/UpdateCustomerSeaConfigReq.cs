using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Customer.Request
{
    public class UpdateCustomerSeaConfigReq
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
        /// 是否启用自动放入公海
        /// </summary>
        public bool Enable { get; set; }



        /// <summary>
        /// 每天最多认领数量
        /// </summary>
        public int ReceiveMaxLimit { get; set; }

        /// <summary>
        /// 业务员达到可认领的入职时长最大值
        /// </summary>
        public int ReceiveJobMax { get; set; }

        /// <summary>
        /// 业务员达到可认领的入职时长最小值
        /// </summary>
        public int ReceiveJobMin { get; set; }

        /// <summary>
        /// 是否启用公海认领分配规则
        /// </summary>
        public bool ReceiveEnable { get; set; }



        /// <summary>
        /// 主动放入规则天数
        /// </summary>
        public int AutomaticDayLimit { get; set; }

        /// <summary>
        /// 是否启用主动放入规则
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
