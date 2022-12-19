using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using OpenAuth.Repository.Core;


namespace OpenAuth.Repository.Domain
{
    [Table("RateDetail")]
    public partial class RateDetail : Entity
    {
        /// <summary>
        /// 月份
        /// </summary>
        [Description("月份")]
        public string Time { get; set; }

        /// <summary>
        /// 数据
        /// </summary>
        [Description("数据")]
        public string Data { get; set; }

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
        /// 是否删除
        /// </summary>
        [Description("是否删除")]
        public int IsDelete { get; set; }


        public int ScriptFlag { get; set; }

    }


}
