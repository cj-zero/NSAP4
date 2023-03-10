using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Serve.Request
{
    public class AppUserReq
    {
        public List<int> user_ids { get; set; }
        public string key { get; set; }
        public string department { get; set; }
        public string department_id { get; set; }
        public string username { get; set; }
        public int page_index { get; set; }
        public int page_size { get; set; }
    }
}
