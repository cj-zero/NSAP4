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

        /// <summary>
        /// 排序字段EntryTime/Count
        /// </summary>
        public string Sort { get; set; }


        /// <summary>
        /// 正序Asc /倒叙Desc
        /// </summary>
        public string SortType { get; set; }
    }
}
