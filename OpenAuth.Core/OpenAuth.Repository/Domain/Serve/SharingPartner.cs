using OpenAuth.Repository.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace OpenAuth.Repository.Domain.Serve
{
    /// <summary>
    /// 共享伙伴
    /// </summary>
    [Table("sharingpartner")]
    public class SharingPartner: Entity
    {
        public SharingPartner() 
        {

        }
        /// <summary>
        /// 客户代码
        /// </summary>
        [Description("客户代码")]
        public string CardCode { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        [Description("创建时间")]
        public DateTime CreateTime { get; set; }
        /// <summary>
        /// 创建人姓名
        /// </summary>
        [Description("创建人姓名")]
        public string CreateUserName { get; set; }
        /// <summary>
        /// 创建人id
        /// </summary>
        [Description("创建人id")]
        public string CreateUserId { get; set; }
    }
}
