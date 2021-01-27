using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using OpenAuth.Repository.Core;

namespace OpenAuth.App.Material.Request
{
    /// <summary>
	/// 
	/// </summary>
    [Table("salesorderwarrantydate")]
    public partial class AddOrUpdatesalesorderwarrantydateReq 
    {
        /// <summary>
        /// id
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// 保修结束时间
        /// </summary>
        public System.DateTime? WarrantyPeriod { get; set; }

        /// <summary>
        /// 是否通过
        /// </summary>
        public bool? IsPass { get; set; }
        //todo:添加自己的请求字段
    }
}