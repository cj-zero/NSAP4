using OpenAuth.Repository.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace OpenAuth.Repository.Domain
{
    [Table("reportinfo")]
    public class ReportInfo :  Entity
    {
        /// <summary>
        /// 报表编码
        /// </summary>
        public string Code { get; set; }
        /// <summary>
        /// 报表名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 大小
        /// </summary>
        public string Size { get; set; }
        /// <summary>
        /// 排序号
        /// </summary>
        public int Sort { get; set; }
    }
}
