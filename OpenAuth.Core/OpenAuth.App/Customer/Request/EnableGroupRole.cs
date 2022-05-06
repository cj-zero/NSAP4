using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Customer.Request
{
    public class EnableGroupRoleReq
    {
        /// <summary>
        /// 组规则id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 启用或者禁用
        /// </summary>
        public bool Enable { get; set; }
    }
}
