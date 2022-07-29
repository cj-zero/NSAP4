using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Customer.Response
{
    public class QueryCustomerSalerListResponse
    {
        /// <summary>
        /// 销售员(归属人)Id
        /// </summary>
        public int SlpCode { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int? LogInstanc { get; set; }

        /// <summary>
        /// 归属人名称
        /// </summary>
        public string SalerName { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime? CreateTime { get; set; }

        /// <summary>
        /// 领取时间
        /// </summary>
        public DateTime? ReceiveDate { get; set; }

        /// <summary>
        /// 释放时间
        /// </summary>
        public DateTime? ReleaseDate { get; set; }

        /// <summary>
        /// 掉入公海时间
        /// </summary>
        public DateTime? FallIntoDate { get; set; }


        #region 黑名单历史归属
        public int type { get; set; }

        public string movein_type { get; set; }

        public string remark { get; set; }

        #endregion
    }
}
