using OpenAuth.Repository.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace OpenAuth.Repository.Domain
{
    [Table("fromthemerelevant")]
    public class FromThemeRelevant : Entity
    {
        public string FromThemeCode { get; set; }
        public int? ServiceMode { get; set; }
        public int Count { get; set; }
    }
}
