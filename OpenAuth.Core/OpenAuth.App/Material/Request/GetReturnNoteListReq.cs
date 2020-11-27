using OpenAuth.App.Request;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Material.Request
{
    public class GetReturnNoteListReq : PageReq
    {
        public string Id { get; set; }

        public string Customer { get; set; }

        public string SapId { get; set; }

        public string CreaterName { get; set; }

        public string BeginDate { get; set; }

        public string EndDate { get; set; }

        public string Status { get; set; }
    }
}
