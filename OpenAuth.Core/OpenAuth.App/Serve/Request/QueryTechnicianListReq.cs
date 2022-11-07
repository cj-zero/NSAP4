using OpenAuth.App.Request;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Serve.Request
{
    public class QueryTechnicianListReq : PageReq
    {
        /// <summary>
        /// 名字
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 部门
        /// </summary>
        public string Org { get; set; }
        /// <summary>
        /// 等级
        /// </summary>
        public string Grade { get; set; }
    }
}
