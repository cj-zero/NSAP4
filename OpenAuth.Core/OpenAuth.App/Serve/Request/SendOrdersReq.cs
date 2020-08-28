using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace OpenAuth.App.Request
{
    public class SendOrdersReq
    {
        
        public List<int> WorkOrderIds { get; set; }

        public string ServiceOrderId { get; set; }

        public List<string> QryMaterialTypes { get; set; }

        [Required]
        public int CurrentUserId { get; set; }
    }
}
