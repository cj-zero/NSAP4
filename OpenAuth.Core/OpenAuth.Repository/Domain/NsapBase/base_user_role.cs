using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace OpenAuth.Repository.Domain.NsapBase
{
    [Table("base_user_role")]
    public partial class base_user_role
    {
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int user_id { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int role_id { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public DateTime upd_dt { get; set; }
        
    }
}
