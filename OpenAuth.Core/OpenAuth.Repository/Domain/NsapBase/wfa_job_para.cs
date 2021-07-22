using OpenAuth.Repository.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace OpenAuth.Repository.Domain.NsapBase
{
    /// <summary>
    /// 流程参数
    /// </summary>
    [Table("wfa_job_para")]
    public class WfaJobPara : Entity
    {
        /// <summary>
        /// 流程类型ID
        /// </summary>
        public int job_id { get; set; }
        /// <summary>
        /// 参数序号(从1开始)
        /// </summary>
        public int para_idx { get; set; }
        /// <summary>
        /// 参数值
        /// </summary>
        public string para_val { get; set; }
        /// <summary>
        /// 更新时间
        /// </summary>
        public DateTime upd_dt { get; set; }
    }
}
