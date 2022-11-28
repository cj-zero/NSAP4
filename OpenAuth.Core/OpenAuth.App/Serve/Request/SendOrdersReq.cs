using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace OpenAuth.App.Request
{
    public class SendOrdersReq
    {

        public string ServiceOrderId { get; set; }

        public List<string> QryMaterialTypes { get; set; } =new List<string>();

        [Required]
        public int CurrentUserId { get; set; }

        /// <summary>
        /// 派单类型 0正常派单 1转派
        /// </summary>
        public int Type { get; set; }
    }

    public class TakeOrder
    {
        /// <summary>
        /// 
        /// </summary>
        public int AppUserId { get; set; }
        public int ServiceOrderId { get; set; }
    }
}
