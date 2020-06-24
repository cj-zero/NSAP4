using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Response
{
    public class CertinfoView
    {
        /// <summary>
        /// Id
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// 证书编号
        /// </summary>
        public string CertNo { get; set; }
        /// <summary>
        /// 证书审批状态
        /// </summary>
        public string ActivityName { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public System.DateTime CreateTime { get; set; }
    }
}
