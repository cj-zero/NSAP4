﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace OpenAuth.App.Request
{
    public class AppQueryServiceOrderListReq : PageReq
    {
        /// <summary>
        /// APP用户Id
        /// </summary>
        [Required]
        public int AppUserId { get; set; }

        /// <summary>
        /// 类型 0默认 1评价列表
        /// </summary>
        [Required]
        public int Type { get; set; }

    }
}
