using OpenAuth.App.Request;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace OpenAuth.App.Serve.Request
{
    public class GetServiceOrderMessageListReq : PageReq
    {
        /// <summary>
        /// 技术员Id
        /// </summary>
        [Required]
        public int CurrentUserId { get; set; }
    }
}
