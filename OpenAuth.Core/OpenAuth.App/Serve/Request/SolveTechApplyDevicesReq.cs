using OpenAuth.App.Request;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Serve.Request
{
    public class SolveTechApplyDevicesReq
    {
        /// <summary>
        /// 解决类型 0不处理 1修改 2新增
        /// </summary>
        public int SolveType { get; set; }

        /// <summary>
        /// 申请Id
        /// </summary>
        public string ApplyId { get; set; }

        public AddServiceWorkOrderReq addServiceWorkOrder { get; set; }
    }
}
