using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Settlement.Request
{
    public class CreateReportReq
    {
        public string Name { get; set; }
        public List<int> Ids { get; set; }
    }

    public class OutsourcCondition
    {
        public int? Id { get; set; }
        public int? OutsourcReportId { get; set; }
        public string StatusName { get; set; }
    }

    public class TechnicianGrades
    {
        public int AppUserId { get; set; }
        public int GradeName { get; set; }
    }
}
