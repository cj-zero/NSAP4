using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Material.Request
{
    public class InternalContactDeptInfoReq
    {
        /// <summary>
        /// 部门ID
        /// </summary>
        public string OrgId { get; set; }
        /// <summary>
        /// 部门名称
        /// </summary>
        public string OrgName { get; set; }
        /// <summary>
        /// true.固定查收 flase.审批查收
        /// </summary>
        public bool Flag { get; set; }
    }
}
