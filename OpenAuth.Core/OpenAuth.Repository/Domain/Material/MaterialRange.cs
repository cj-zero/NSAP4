using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace OpenAuth.Repository.Domain
{
    [Table("materialrange")]
    public class MaterialRange
    {
        public string ItemCode { get; set; }
        public string? Volt { get; set; }
        public string? Amp { get; set; }
        public string Unit { get; set; }
        public string Prefix { get; set; }
    }
}
