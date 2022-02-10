using System;
using System.Collections.Generic;
using System.Text;
using OpenAuth.App.Request;

namespace OpenAuth.App.Client.Request
{
    public class ClientListReq:PageReq
    {
        public string qtype { get; set; }
        public string query { get; set; }
        public string sortname { get; set; }
        public string sortorder { get; set; }
    }
}
