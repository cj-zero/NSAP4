//------------------------------------------------------------------------------
// <autogenerated>
//     This code was generated by a CodeSmith Template.
//
//     DO NOT MODIFY contents of this file. Changes to this
//     file will be lost if the code is regenerated.
//     Author:RenChun Xia
// </autogenerated>
//------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using OpenAuth.Repository.Core;

namespace OpenAuth.Repository.Domain
{
    /// <summary>
    /// 钉钉用户部门信息表
    /// </summary>
    [Table("dduserdepartmsg")]
    public partial class DDUserDepartMsg : Entity
    {
        public DDUserDepartMsg()
        {
            this.DepartId = 0;
            this.DepartName = string.Empty;
            this.UserId = string.Empty;
            this.UserName = string.Empty;
            this.UserPhone = string.Empty;
            this.CreateUserId = string.Empty;
            this.CreateName = string.Empty;
            this.IsBind = false;
        }

        /// <summary>
        /// 部门Id
        /// </summary>
        [Description("部门Id")]
        public int? DepartId { get; set; }

        /// <summary>
        /// 部门名称
        /// </summary>
        [Description("部门名称")]
        public string DepartName { get; set; }

        /// <summary>
        /// 用户Id
        /// </summary>
        [Description("用户Id")]
        public string UserId { get; set; }

        /// <summary>
        /// 用户名称
        /// </summary>
        [Description("用户名称")]
        public string UserName { get; set; }

        /// <summary>
        /// 用户手机号
        /// </summary>
        [Description("用户手机号")]
        public string UserPhone { get; set; }

        /// <summary>
        /// 是否绑定
        /// </summary>
        [Description("是否绑定")]
        public bool? IsBind { get; set; }

        /// <summary>
        /// 创建人Id
        /// </summary>
        [Description("创建人Id")]
        public string CreateUserId { get; set; }

        /// <summary>
        /// 创建人
        /// </summary>
        [Description("创建人")]
        public string CreateName { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        [Description("创建时间")]
        public System.DateTime? CreateTime { get; set; }
    }
}
