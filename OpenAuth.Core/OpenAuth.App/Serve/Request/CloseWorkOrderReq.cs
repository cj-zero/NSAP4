using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace OpenAuth.App.Serve.Request
{
    public class CloseWorkOrderReq
    {
        /// <summary>
        /// 工单Id
        /// </summary>
        [Required]
        public int Id { get; set; }

        /// <summary>
        /// 关单原因
        /// </summary>
        public string Reason { get; set; }

        /// <summary>
        /// 当前操作人Id
        /// </summary>
        [Required]
        public int CurrentUserId { get; set; }
    }
}
