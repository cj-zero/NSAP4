using OpenAuth.Repository.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace OpenAuth.Repository.Domain.Settlement
{
    /// <summary>
	/// 
	/// </summary>
    [Table("outsourcreport")]
    public class OutsourcReport : BaseEntity<int>
    {
        public OutsourcReport()
        {
            this.BatchNo = string.Empty;
            this.CreateUserId = string.Empty;
            this.CreateUser = string.Empty;
            this.CreateTime = DateTime.Now;
            this.Name = string.Empty;
        }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string BatchNo { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        [Browsable(false)]
        public string CreateUserId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string CreateUser { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string Name { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public System.DateTime? CreateTime { get; set; }

        public int? Status { get; set; }
        public decimal? TotalMoney { get; set; }
        public string MakerList { get; set; }
        public string ProcessedList { get; set; }

        public override void GenerateDefaultKeyVal()
        {

        }

        public override bool KeyIsNull()
        {
            return Id == 0;
        }
    }
}
