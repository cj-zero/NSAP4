using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using OpenAuth.Repository.Core;

namespace OpenAuth.Repository.Domain.Serve
{
    /// <summary>
	/// 技术员完成服务单数
	/// </summary>
    [Table("techniciancompletedquantity")]
    public partial class TechnicianCompletedQuantity : Entity
    {
        public TechnicianCompletedQuantity()
        {
            
        }

        /// <summary>
        /// 年份
        /// </summary>
        public int Year { get; set; }
        /// <summary>
        /// 月份
        /// </summary>
        public int Month { get; set; }
        /// <summary>
        /// 完成数量
        /// </summary>
        public int? Quantity { get; set; }
        /// <summary>
        /// 技术员ID
        /// </summary>
        public string UserId { get; set; }
        /// <summary>
        /// 技术员姓名
        /// </summary>
        public string UserName { get; set; }
    }
}
