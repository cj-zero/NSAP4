using OpenAuth.App.Request;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Sap.Request
{
    public class QueryBusinessPartnerListReq: PageReq
    {
        /// <summary>
        /// 客户代码或者客户名称
        /// </summary>
        public string CardCodeOrCardName { get; set; }
    }
}
