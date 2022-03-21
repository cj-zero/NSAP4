using OpenAuth.Repository.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace OpenAuth.Repository.Domain
{

    [Table("internalcontacttaskserviceorder")]
    public class InternalContactTaskServiceOrder : Entity
    {
        public InternalContactTaskServiceOrder()
        {
            this.InternalContactTaskId = string.Empty;
        }
        /// <summary>
        /// 内联单任务单ID
        /// </summary>
        public string InternalContactTaskId { get; set; }
        /// <summary>
        /// 服务ID
        /// </summary>
        public int? ServiceOrderId { get; set; }
        public bool IsFinish { get; set; }
    }
}
