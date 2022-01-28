using Infrastructure.AutoMapper;
using OpenAuth.Repository.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Material.Request
{
    [AutoMapTo(typeof(InternalContactProduction))]
    public class InternalContactProductionReq
    {
        public int? InternalContactId { get; set; }
        public int ProductionId { get; set; }
        public string ItemCode { get; set; }
        public int BelongQty { get; set; }
    }
}
