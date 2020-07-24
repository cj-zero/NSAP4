using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace OpenAuth.App.Request
{
    public class SendOrdersReq
    {
        [Required]
        public List<int> WorkOrderIds { get; set; }

        [Required]
        public int CurrentUserId { get; set; }
    }
}
