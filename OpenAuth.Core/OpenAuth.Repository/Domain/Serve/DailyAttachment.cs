using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using OpenAuth.Repository.Core;

namespace OpenAuth.Repository.Domain
{
    /// <summary>
	/// 
	/// </summary>
    [Table("dailyattachment")]
    public partial class DailyAttachment : Entity
    {
        public DailyAttachment()
        {
            this.FileId = string.Empty;
            this.ExpendId = string.Empty;
        }


        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        [Browsable(false)]
        public string FileId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        [Browsable(false)]
        public string ExpendId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? Type { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? AttachmentType { get; set; }
    }
}