using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Request
{
    public class ModifyServiceOrderReq
    {
        public int Id { get; set; }
        /// <summary>
        /// 最新联系人
        /// </summary>
        public string NewestContacter { get; set; }
        /// <summary>
        /// 最新联系人电话号码
        /// </summary>
        public string NewestContactTel { get; set; }
        /// <summary>
        /// 终端客户
        /// </summary>
        public string TerminalCustomer { get; set; }
        /// <summary>
        /// 终端客户ID
        /// </summary>
        public string TerminalCustomerId { get; set; }
        /// <summary>
        /// 省
        /// </summary>
        public string Province { get; set; }
        /// <summary>
        /// 市
        /// </summary>
        public string City { get; set; }
        /// <summary>
        /// 区
        /// </summary>
        public string Area { get; set; }
        /// <summary>
        /// 详细地址
        /// </summary>
        public string Addr { get; set; }
        /// <summary>
        /// 地址标识
        /// </summary>
        public string AddressDesignator { get; set; }
        /// <summary>
        /// 地址
        /// </summary>
        public string Address { get; set; }
    }
}
