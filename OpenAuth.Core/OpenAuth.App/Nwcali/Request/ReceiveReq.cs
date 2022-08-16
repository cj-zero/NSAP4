using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Nwcali.Request
{
    public class ReceiveReq
    {
        public string GeneratorCode { get; set; }
        public int? ReceiveNo { get; set; }
        public int? Operator { get; set; }
        public DateTime? OperateTime { get; set; }
    }
}
