using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Customer.Response
{
    public class QueryCustomerSeaListResponse
    {
        /// <summary>
        /// 客户代码
        /// </summary>
        public string CustomerNo { get; set; }

        /// <summary>
        /// 客户名称
        /// </summary>
        public string CustomerName { get; set; }

        /// <summary>
        /// 显示的客户代码
        /// </summary>
        public string DisplayCustomerNo { get; set; }

        /// <summary>
        /// 显示的客户名称
        /// </summary>
        public string DisplayCustomerName { get; set; }

        /// <summary>
        /// 部门
        /// </summary>
        public string DepartMent { get; set; }

        /// <summary>
        /// 销售员代码
        /// </summary>
        public int SlpCode { get; set; }

        /// <summary>
        /// 销售员名称
        /// </summary>
        public string SlpName { get; set; }

        /// <summary>
        /// 上一次交易时间
        /// </summary>
        public string LastOrderTime { get; set; }

        /// <summary>
        /// 总交易金额
        /// </summary>
        public decimal? TotalMoney { get; set; }

        /// <summary>
        /// 被领取次数
        /// </summary>
        public int? ReceiveTime { get; set; }

        /// <summary>
        /// 创建人
        /// </summary>
        public string CreateUser { get; set; }

        /// <summary>
        /// 创建名称
        /// </summary>
        public DateTime CreateDateTime { get; set; }

        /// <summary>
        /// 掉入时间
        /// </summary>
        public DateTime FallIntoTime { get; set; }
    }
}
