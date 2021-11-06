using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Request
{
    public class OrgTree
    {
        public string Name { get; set; }
        public List<object> Node { get; set; }
    }

    public class UserOrg
    {
        public string Name { get; set; }
        public string SecondId { get; set; }
        public int? AppUserId { get; set; }
    }
}
