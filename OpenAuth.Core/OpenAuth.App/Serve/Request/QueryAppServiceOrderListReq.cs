using OpenAuth.App.Request;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace OpenAuth.App.Serve.Request
{
    public class QueryAppServiceOrderListReq : PageReq
    {
        /// <summary>
        /// 呼叫状态查询条件 0:全部 1待派单 2已派单
        /// </summary>
        [Required]
        public string QryState { get; set; }
    }
}
