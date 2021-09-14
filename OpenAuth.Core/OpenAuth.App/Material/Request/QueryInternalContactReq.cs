using OpenAuth.App.Request;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Material.Request
{
    public class QueryInternalContactReq : PageReq
    {
        /// <summary>
        /// IW号
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// 主题
        /// </summary>
        public string Theme { get; set; }
        /// <summary>
        /// 接收部门
        /// </summary>
        public string ReceiveOrg { get; set; }
        /// <summary>
        /// 执行部门
        /// </summary>
        public string ExecOrg { get; set; }
        /// <summary>
        /// 客户代码
        /// </summary>
        public string CardCode { get; set; }
        /// <summary>
        /// 创建人
        /// </summary>
        public string CreateUser { get; set; }
        /// <summary>
        /// 状态
        /// </summary>
        public int? Status { get; set; }
        /// <summary>
        /// 页面类型
        /// </summary>
        public int PageType { get; set; }
    }
}
