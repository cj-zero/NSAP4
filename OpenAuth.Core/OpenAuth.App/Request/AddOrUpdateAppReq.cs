using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Request
{
    public class AddOrUpdateAppReq
    {
        public string Id { get; set; }
        /// <summary>
        /// 应用名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 应用密钥
        /// </summary>
        public string AppSecret { get; set; }
        /// <summary>
        /// AppKey
        /// </summary>
        public string AppKey { get; set; }
        /// <summary>
        /// 应用描述
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// 应用图标
        /// </summary>
        public string Icon { get; set; }
        /// <summary>
        /// ReturnUrl
        /// </summary>
        public string ReturnUrl { get; set; }
        /// <summary>
        /// 是否可用
        /// </summary>
        public bool Disable { get; set; }
        /// <summary>
        /// 创建日期
        /// </summary>
        public System.DateTime CreateTime { get; set; }
        /// <summary>
        /// 创建人
        /// </summary>
        public string CreateUser { get; set; }
    }
}
