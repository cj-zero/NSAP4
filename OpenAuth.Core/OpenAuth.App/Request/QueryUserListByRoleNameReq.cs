using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace OpenAuth.App.Request
{
    public class QueryUserListByRoleNameReq : PageReq
    {
        [Required]
        public string RoleName { get; set; }

        public string UserName { get; set; }
    }
}
