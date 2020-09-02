using System;
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
        /// 类型 0默认全部 1待受理 2已受理 3待评价 4已评价
        /// </summary>
        [Required]
        public int Type { get; set; }

    }
}
