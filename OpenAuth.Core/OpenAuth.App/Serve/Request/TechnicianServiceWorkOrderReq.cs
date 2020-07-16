using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace OpenAuth.App.Request
{
    public class TechnicianServiceWorkOrderReq : PageReq
    {
        [Required]
        public int TechnicianId { get; set; }
    }
}
