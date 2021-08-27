using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Response
{
    public partial class OrgMansgerResp
    {
        public string OrgId { get; set; }
        public string[] UserId { get; set; }
        public string[] UserName { get; set; }
    }
}
