using OpenAuth.App.Request;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace OpenAuth.App.Serve.Request
{
    public class GetSalesManServiceOrderReq : PageReq
    {
        /// <summary>
        /// 技术员Id
        /// </summary>
        [Required]
        public int AppUserId { get; set; }

        /// <summary>
        /// 状态类型 1-进行中 2已完成
        /// </summary>
        [Required]
        public int Type { get; set; }
    }
}
