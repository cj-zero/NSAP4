using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Request
{
    public class QueryLocationInfoReq
    {
        public string Province { get; set; }
        public string City { get; set; }
        public string Area { get; set; }
        public List<string> Name { get; set; }

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
