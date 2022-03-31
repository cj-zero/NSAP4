using Infrastructure.AutoMapper;
using OpenAuth.Repository.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Material.Request
{
    [AutoMapTo(typeof(InternalContactProductionDetail))]
    public class InternalContactProductionDetailReq
    {
        public string InternalContactProductionId { get; set; }
        /// <summary>
        /// 序列号
        /// </summary>
        public string MnfSerial { get; set; }
        public string WhsCode { get; set; }
        public string OrgName { get; set; }
        public int? Quantity { get; set; }
    }
}
