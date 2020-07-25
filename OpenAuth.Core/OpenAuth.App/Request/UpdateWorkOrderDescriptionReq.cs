using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace OpenAuth.App.Request
{
    public class UpdateWorkOrderDescriptionReq
    {
        /// <summary>
        /// 工单Id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 描述类型 Trouble故障 Process过程
        /// </summary>
        public string DescriptionType { get; set; }

        /// <summary>
        /// 描述内容
        /// </summary>
        [MaxLength(500)]
        public string Description { get; set; }

    }
}
