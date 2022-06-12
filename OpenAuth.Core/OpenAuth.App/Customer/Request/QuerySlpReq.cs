using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Customer.Request
{
    public class QuerySlpReq
    {
        /// <summary>
        /// 部门
        /// </summary>
        public string Dept { get; set; }

        /// <summary>
        /// 账号
        /// </summary>
        public string Account { get; set; }

        /// <summary>
        /// 姓名
        /// </summary>
        public string Name { get; set; }
    }
}
