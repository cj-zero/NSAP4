using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Order
{
    public class CardInfoDto
    {
        /// <summary>
        /// 联系人
        /// </summary>
        public string CntctPrsn { get; set; }
        /// <summary>
        /// 业务伙伴名称
        /// </summary>
        public string CardName { get; set; }
        /// <summary>
        /// 收货方
        /// </summary>
        public string ShipToCode { get; set; }
        /// <summary>
        /// 收货地址
        /// </summary>
        public string Address2 { get; set; }
        /// <summary>
        /// 付款方
        /// </summary>
        public string PayToCode { get; set; }
        /// <summary>
        /// 付款地址
        /// </summary>
        public string Address { get; set; }
        /// <summary>
        /// 业务员
        /// </summary>
        public object U_YWY { get; set; }
        /// <summary>
        /// 货币类型
        /// </summary>

        public string Currency { get; set; }
        /// <summary>
        /// 发票类型
        /// </summary>
        public string U_FPLB { get; set; }
    }
}
