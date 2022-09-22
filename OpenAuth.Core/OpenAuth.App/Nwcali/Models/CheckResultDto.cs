using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Nwcali
{
    public class CheckResultDto
    {
        public int? ChlId { get; set; }
        public string TaskId { get; set; }
        public int? LastTime { get; set; }
        public int Status { get; set; }
        public int ErrCount { get; set; }
        public List<CheckResultItem> CheckItems { get; set; }
    }

    public class CheckResultItem
    {
        public string CheckId { get; set; }
        public int CheckType { get; set; }
        public int? LastTime { get; set; }
        public int Status { get; set; }
        public int ErrCount { get; set; }
        public List<CheckResultItemRecord> Records { get; set; }
    }

    public class CheckResultItemRecord
    {
        public int? BegSeq { get; set; }
        public int? EndSeq { get; set; }
        public string Err { get; set; }
        public int? CheckTime { get; set; }
    }
}
