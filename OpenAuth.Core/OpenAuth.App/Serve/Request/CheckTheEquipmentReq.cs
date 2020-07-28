using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace OpenAuth.App.Request
{
    public class CheckTheEquipmentReq
    {
        public int ServiceOrderId { get; set; }

        public string ErrorWorkOrderIds { get; set; }

        public string CheckWorkOrderIds { get; set; }

        [Required]
        public int CurrentUserId { get; set; }
    }
}
