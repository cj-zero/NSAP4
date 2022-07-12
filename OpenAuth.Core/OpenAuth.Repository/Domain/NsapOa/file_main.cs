using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace OpenAuth.Repository.Domain
{
    [Table("file_main")]
    public class file_main
    {
        public int? file_id { get; set; }
        public string file_sn { get; set; }
        public string file_nm { get; set; }
        public int? file_type_id { get; set; }
        public int? docEntry { get; set; }
        public int? job_id { get; set; }
        public string file_ver { get; set; }
        public int? acct_id { get; set; }
        public string issue_reason { get; set; }
        public string file_path { get; set; }
        public string content { get; set; }
        public string remarks { get; set; }
        public int? file_status { get; set; }
        public DateTime? upd_dt { get; set; }
        public string view_file_path { get; set; }
        public int sbo_id { get; set; }
    }
}
