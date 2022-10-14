using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using OpenAuth.Repository.Core;

namespace OpenAuth.Repository.Domain
{
    /// <summary>
    /// JOB客户关系 
    /// </summary>
    [Table("JobClientRelation")]
    public partial  class JobClientRelation : Entity
    {
        /// <summary>
        /// 任务编号
        /// </summary>
        public int Jobid { get; set; }
        /// <summary>
        /// 终端关系
        /// </summary>
        public string Terminals { get; set; }

        /// <summary>
        /// 创建人
        /// </summary>
        [Description("创建人")]
        public string Creator { get; set; }

        /// <summary>
        /// 创建人
        /// </summary>
        [Description("创建人id")]
        public string CreatorId{get; set; }

        /// <summary>
        /// 创建日期
        /// </summary>
        [Description("创建日期")]
        public System.DateTime CreateDate { get; set; }

        /// <summary>
        /// 是否删除： 0 否 1 是
        /// </summary>
        [Description("是否删除")]
        public int IsDelete { get; set; }


    }


}
