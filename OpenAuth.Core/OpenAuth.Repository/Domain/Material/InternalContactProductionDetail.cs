using OpenAuth.Repository.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace OpenAuth.Repository.Domain
{
    [Table("internalcontactproductiondetail")]
    public class InternalContactProductionDetail : Entity
    {
        public InternalContactProductionDetail()
        {
            
        }

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
