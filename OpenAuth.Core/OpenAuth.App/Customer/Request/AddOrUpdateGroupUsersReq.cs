﻿using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Customer.Request
{
    public class AddOrUpdateGroupUsersReq
    {
        /// <summary>
        /// 规则id
        /// </summary>
        public int Id { get; set; }

        public IEnumerable<User> Users { get; set; }
    }

    public class User
    {
        /// <summary>
        /// 用户id
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// 用户姓名
        /// </summary>
        public string UserName { get; set; }
    }
}
