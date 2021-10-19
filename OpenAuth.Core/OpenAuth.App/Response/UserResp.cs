using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Response
{
    public class UserResp
    {
        /// <summary>
        /// 用户ID
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// 用户名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 部门ID
        /// </summary>
        public string OrgId { get; set; }
        /// <summary>
        /// 部门名称
        /// </summary>
        public string OrgName { get; set; }
        public string CascadeId { get; set; }
    }
}
