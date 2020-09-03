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
       
        /// <summary>
        /// 制造商序列号
        /// </summary>
        public string ManufSN { get; set; }

        /// <summary>
        /// 售后主管
        /// </summary>
        public string Technician { get; set; }

        /// <summary>
        /// 销售员
        /// </summary>
        public string slpName { get; set; }

        /// <summary>
        /// 收货地址
        /// </summary>
        public string Address { get; set; }
    }
}
