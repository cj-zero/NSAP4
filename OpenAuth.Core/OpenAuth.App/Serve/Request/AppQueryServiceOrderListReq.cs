using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace OpenAuth.App.Request
{
    public class AppQueryServiceOrderListReq : PageReq
    {
        [Required]
        public int AppUserId { get; set; }

    }
}
