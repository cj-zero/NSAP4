using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace OpenAuth.App.Request
{
    public class AppQueryServiceOrderReq
    {

        [Required]
        public int AppUserId { get; set; }

        [Required]
        public int ServiceOrderId { get; set; }
    }
}
