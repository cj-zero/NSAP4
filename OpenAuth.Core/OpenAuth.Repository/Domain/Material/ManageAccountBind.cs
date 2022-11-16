using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using OpenAuth.Repository.Core;

namespace OpenAuth.Repository.Domain.Material
{
    [Table("manageaccountbind")]
    public partial class ManageAccountBind : Entity
    {
        /// <summary>
        /// manage账号
        /// </summary>
        [Description("manage账号")]
        public string MAccount { get; set; }

        /// <summary>
        /// manage姓名
        /// </summary>
        [Description("manage姓名")]
        public string MName { get; set; }


        /// <summary>
        /// 4.0账号
        /// </summary>
        [Description("4.0账号")]
        public string LAccount { get; set; }

        /// <summary>
        ///4.0姓名
        /// </summary>
        [Description("4.0姓名")]
        public string LName { get; set; }


        /// <summary>
        /// 岗位级别
        /// </summary>
        [Description("岗位级别")]
        public string Level { get; set; }

        /// <summary>
        /// 是否考勤
        /// </summary>
        [Description("是否考勤")]
        public int DutyFlag { get; set; }

        /// <summary>
        /// 创建日期
        /// </summary>
        [Description("创建日期")]
        public System.DateTime CreateDate { get; set; }

        /// <summary>
        /// 更新日期
        /// </summary>
        [Description("更新日期")]
        public System.DateTime UpdateDate { get; set; }

        /// <summary>
        /// 创建人
        /// </summary>
        [Description("创建人")]
        public string Creator { get; set; }

        /// <summary>
        /// 创建人id
        /// </summary>
        [Description("创建人id")]
        public string Creatorid { get; set; }

        /// <summary>
        /// 变更人
        /// </summary>
        [Description("变更人")]
        public string Updater { get; set; }

        /// <summary>
        /// 变更人id
        /// </summary>
        [Description("变更人id")]
        public string Updaterid { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        [Description("备注")]
        public string Remark { get; set; }

        /// <summary>
        /// 是否删除： 0 否 1 是
        /// </summary>
        [Description("是否删除")]
        public int IsDelete { get; set; }


    }
}
