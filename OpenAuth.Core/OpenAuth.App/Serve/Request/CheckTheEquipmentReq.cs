using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace OpenAuth.App.Request
{
    public class CheckTheEquipmentReq
    {

        [Required]
        public int WorkOrderId { get; set; }

        [Required]
        public int CurrentUserId { get; set; }
    }
}
