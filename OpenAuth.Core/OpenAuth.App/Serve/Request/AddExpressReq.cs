using OpenAuth.App.Request;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Serve.Request
{
    public class AddExpressReq
    {
        /// <summary>
        /// 返厂Id
        /// </summary>
        public int ReturnRepairId { get; set; }

        /// <summary>
        /// 快递单号
        /// </summary>
        public string TrackNum { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }

        /// <summary>
        /// 附件集合
        /// </summary>
        public List<string> Accessorys { get; set; }
    }
}
