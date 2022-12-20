using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Request
{
    public class OrgTree
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public List<object> Node { get; set; }
    }

    public class UserOrg
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string SecondId { get; set; }
        public string SecondName { get; set; }
        public int? AppUserId { get; set; }
    }

    public class ModuleTree
    {
        public string Code { get; set; }
        public string Name { get; set; }

        public List<ModuleTree> Children { get; set; }
    }
}
