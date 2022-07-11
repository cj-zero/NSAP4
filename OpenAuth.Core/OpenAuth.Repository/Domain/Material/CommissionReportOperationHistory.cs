using OpenAuth.Repository.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace OpenAuth.Repository.Domain
{

    [Table("commissionreportoperationhistory")]
    public class CommissionReportOperationHistory : Entity
    {
        public int? CommissionReportId { get; set; }
        public string UserId { get; set; }
        public string StepName { get; set; }
    }
}
