using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Response
{
    public class ReimburseOrgResp
    {
        public string Label { get; set; }

        public string Value { get; set; }

        public List<ReimburseOrg> Children { get; set; }

    }

    public class ReimburseOrg
    {
        public string ParentName { get; set; }

        public string Label { get; set; }

        public string Value { get; set; }
    }
}
