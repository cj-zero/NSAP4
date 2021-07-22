using OpenAuth.Repository.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace OpenAuth.Repository.Domain.NsapBase
{
    /// <summary>
    /// 用户操作日志
    /// </summary>
    [Table("base_user_log")]
    public class base_user_log : Entity
    {
        public base_user_log()
        {
            this.opt_cont = string.Empty;
            this.rec_dt = DateTime.Now;

        }
        /// <summary>
        /// 用户ID
        /// </summary>
        public int user_id { get; set; }
        /// <summary>
        /// 功能ID,=0表示基础功能。
        /// </summary>
        public int func_id { get; set; }
        /// <summary>
        /// 操作内容
        /// </summary>
        public string opt_cont { get; set; }
        /// <summary>
        /// 记录时间
        /// </summary>
        public DateTime rec_dt { get; set; }
    }
}
