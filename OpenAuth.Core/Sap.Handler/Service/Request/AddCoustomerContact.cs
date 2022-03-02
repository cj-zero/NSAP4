using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sap.Handler.Service.Request
{
    public class AddCoustomerContact
    {
        /// <summary>
        /// 客户代码
        /// </summary>
        public string CardCode { get; set; }
        public string NewestContactTel { get; set; }
        public string NewestContacter { get; set; }
        public string Address { get; set; }
    }
}
