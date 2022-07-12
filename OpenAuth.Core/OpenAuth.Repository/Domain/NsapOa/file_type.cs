using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace OpenAuth.Repository.Domain
{
    [Table("file_type")]
    public class file_type
    {
        public int? type_id { get; set; }
        public string type_nm { get; set; }
        public int? type_open { get; set; }
        public string type_prex { get; set; }
        public int? parent_id { get; set; }
        public int? func_id { get; set; }
        public string upper_nm { get; set; }
        public int? is_control { get; set; }
        public int? is_audit { get; set; }
        public string remarks { get; set; }
        public DateTime? upd_date { get; set; }
    }
}
