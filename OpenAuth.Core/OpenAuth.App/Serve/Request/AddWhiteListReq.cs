using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Serve.Request
{
    public class AddWhiteListReq
    {
        public bool ServiceIsEnable { get; set; }
        public bool ConfigIsEnable { get; set; }
        public List<string> UserIds { get; set; }
    }
}
