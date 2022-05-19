using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Nwcali.Request
{
    public class UnBindDeviceReq
    {
        /// <summary>
        /// 生产码
        /// </summary>
        public string GeneratorCode { get; set; }
        /// <summary>
        /// 中位机
        /// </summary>
        public string Guid { get; set; }
        /// <summary>
        /// 下位机
        /// </summary>
        public string LowGuid { get; set; }
        /// <summary>
        /// 解绑类型：1.中位机解绑 2.下位机解绑
        /// </summary>
        public int UnBindType { get; set; }
    }
}
