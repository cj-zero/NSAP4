using OpenAuth.App.Request;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Material.Request
{
   public class GetClearReturnNoteListReq : PageReq
    {
        public string Customer { get; set; }

        public string SapId { get; set; }

        public string CreaterName { get; set; }

        public string BeginDate { get; set; }

        public string EndDate { get; set; }
    }
}
