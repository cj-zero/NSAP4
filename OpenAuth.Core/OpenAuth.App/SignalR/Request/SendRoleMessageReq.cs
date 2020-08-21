using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.SignalR.Request
{
    public class SendRoleMessageReq
    {
        public string Role { get; set; }
        public string Message { get; set; }
    }
}
