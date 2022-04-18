using Infrastructure.AutoMapper;
using OpenAuth.Repository.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Material.Request
{

    [AutoMapTo(typeof(InternalContactTask))]
    public class InternalContactTaskReq
    {
        public string ItemCode { get; set; }
        public int? ProductionId { get; set; }
        public string FromTheme { get; set; }
        public string ProductionOrg { get; set; }
        public string ProductionOrgManager { get; set; }
        public string WareHouse { get; set; }
        public int? BelongQty { get; set; }
        /// <summary>
        /// 整改量
        /// </summary>
        public int? RectifyQty { get; set; }
        public string Remark { get; set; }
    }
}
