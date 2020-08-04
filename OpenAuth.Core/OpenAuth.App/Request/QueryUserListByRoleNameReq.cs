using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace OpenAuth.App.Request
{
    public class QueryUserListByRoleNameReq : PageReq
    {
        /// <summary>
        /// 角色名称集合
        /// </summary>
        [Required]
        public List<string> RoleNames { get; set; }

        public string UserName { get; set; }
    }
}
