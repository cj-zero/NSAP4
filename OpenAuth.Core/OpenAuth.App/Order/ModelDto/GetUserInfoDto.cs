using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Order.ModelDto
{
    public class GetUserInfoDto
    {
        /// <summary>
        /// 用户id
        /// </summary>
        public int userid { get; set; }
        /// <summary>
        /// 用姓名
        /// </summary>
        public string username { get; set; }
        /// <summary>
        /// 角色名称
        /// </summary>
        public List<string> rolename { get; set; }

    }


}
