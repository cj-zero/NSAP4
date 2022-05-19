using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Nwcali.Request
{
    /// <summary>
    /// 
    /// </summary>
    public  class ChannelControlReq
    {
        /// <summary>
        /// 启动类型 1:启动已绑定未测试  2:启动全部绑定
        /// </summary>
        public int type { get; set; }

        /// <summary>
        /// 工步文件地址
        /// </summary>
        public string xmlpath { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string GeneratorCode { get; set; }
    }
}
