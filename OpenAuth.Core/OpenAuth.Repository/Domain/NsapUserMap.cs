using OpenAuth.Repository.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace OpenAuth.Repository.Domain
{
    /// <summary>
    /// 与erp3.0用户绑定表
    /// </summary>
    [Table("NsapUserMap")]
    public class NsapUserMap : Entity
    {
        public NsapUserMap()
        {
            this.UserID = string.Empty;
        }

        /// <summary>
        /// NSAP的UserId
        /// </summary>
        [Description("NSAP的UserId")]
        public string UserID { get; set; }
        /// <summary>
        /// ERP3.0的UserId
        /// </summary>
        [Description("ERP3.0的UserId")]
        [Browsable(false)]
        public int? NsapUserId { get; set; }
    }
}
