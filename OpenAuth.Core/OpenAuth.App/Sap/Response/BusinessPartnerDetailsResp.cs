using OpenAuth.Repository.Domain.Sap;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Response
{
    public class BusinessPartnerDetailsResp
    {
        /// <summary>
        /// 
        /// </summary>
        public string CardCode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string CardName { get; set; }
        /// <summary>
        /// 默认联系人名称
        /// </summary>
        public string CntctPrsn { get; set; }

        /// <summary>
        /// 对应业务员编号
        /// </summary>
        public int SlpCode { get; set; }
        /// <summary>
        /// 对应业务员名称
        /// </summary>
        public string SlpName { get; set; }

        /// <summary>
        /// 对应技术主管编号
        /// </summary>
        public int? TechID { get; set; }
        /// <summary>
        /// 对应技术主管名称
        /// </summary>
        public string TechName { get; set; }

        /// <summary>
        /// 默认联系电话
        /// </summary>
        public string Phone1 { get; set; }

        public List<OCPR> CntctPrsnList { get; set; }

        public List<CRD1> AddressList { get; set; }

    }
}
