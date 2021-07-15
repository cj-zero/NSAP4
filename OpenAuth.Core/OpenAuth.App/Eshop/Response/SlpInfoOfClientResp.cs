using System;
using System.Collections.Generic;
using System.Text;
using OpenAuth.Repository.Domain;

namespace OpenAuth.App.Response
{
    public class SlpInfoOfClientResp
    {
        /// <summary>
        /// 姓名
        /// </summary>
        public string UserName { get; set; }
        /// <summary>
        /// 性别
        /// </summary>
        public string Sex { get; set; }
        /// <summary>
        /// 邮箱
        /// </summary>
        public string Email { get; set; }
        /// <summary>
        /// 手机号
        /// </summary>
        public string Mobile { get; set; }
    }
}
