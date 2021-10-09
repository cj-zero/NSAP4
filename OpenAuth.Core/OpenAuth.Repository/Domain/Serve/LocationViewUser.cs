using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using OpenAuth.Repository.Core;

namespace OpenAuth.Repository.Domain
{
    /// <summary>
    /// 查看名单
    /// </summary>
    [Table("locationviewuser")]
    public partial class LocationViewUser : Entity
    {
        public LocationViewUser()
        {
            this.UserId = string.Empty;
            this.UserName = string.Empty;
        }
        public string UserId { get; set; }
        public string UserName { get; set; }
    }
}
