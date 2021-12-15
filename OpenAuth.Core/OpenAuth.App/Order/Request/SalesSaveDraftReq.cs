using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Order.Request
{
    /// <summary>
    /// 销售交货草稿保存提交
    /// </summary>
    public class SalesSaveDraftReq : AddOrderReq
    {
        public string jobType { get; set; }
    }
}
