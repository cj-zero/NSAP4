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
        public string ItemName { get; set; }
        public string Dept { get; set; }
        public int? PlannedQty { get; set; }
        public int? CmpltQty { get; set; }
        public int? OpenQty { get; set; }
        public string Remark { get; set; }
        public string FromTheme { get; set; }
        public string FromThemeCode { get; set; }
        public string FromThemeName { get; set; }
    }
}
