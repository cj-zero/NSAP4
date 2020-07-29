using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace OpenAuth.App.Request
{
    public class UpdateWorkOrderDescriptionReq
    {
        /// <summary>
        /// 服务单Id
        /// </summary>
        [Required]
        public int ServiceOrderId { get; set; }

        /// <summary>
        /// 描述类型 Trouble故障 Process过程
        /// </summary>
        [Required]
        public string DescriptionType { get; set; }

        /// <summary>
        /// 描述内容
        /// </summary>
        [MaxLength(500)]
        public string Description { get; set; }

        /// <summary>
        /// 当前技术员Id
        /// </summary>
        [Required]
        public int CurrentUserId { get; set; }

    }
}
