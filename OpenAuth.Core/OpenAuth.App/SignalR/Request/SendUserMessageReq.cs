using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.SignalR.Request
{
    public class SendUserMessageReq
    {
        public string UserName { get; set; }

        public string Message { get; set; }
    }
}
