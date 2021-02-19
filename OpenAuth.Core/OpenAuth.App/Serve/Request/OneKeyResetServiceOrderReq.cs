using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Serve.Request
{
    public class OneKeyResetServiceOrderReq
    {
        public int serviceOrderId { get; set; }

        public string Message { get; set; }
    }
}
