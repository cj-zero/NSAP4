using OpenAuth.App.Request;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Meeting.Request
{
    public class DispatcherReq:PageReq
    {
        public int MeetingId { get; set; }
        public string  Name { get; set; }
    }
}
