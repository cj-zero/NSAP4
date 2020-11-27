using System;
using System.Collections.Generic;
using System.Text;

namespace Sap.Handler.Service
{
    public class FromThemeJson
    {
        /// <summary>
        /// 服务呼叫ID
        /// </summary>
        public int? Id { get; set; }

        /// <summary>
        /// 服务呼叫内容
        /// </summary>
        public string description { get; set; }
    }
}
