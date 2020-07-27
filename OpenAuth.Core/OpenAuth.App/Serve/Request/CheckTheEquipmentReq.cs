using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace OpenAuth.App.Request
{
    public class CheckTheEquipmentReq
    {
        public int IsTrue { get; set; }

        [Required]
        public int WorkOrderId { get; set; }

        [Required]
        public int CurrentUserId { get; set; }
    }
}
