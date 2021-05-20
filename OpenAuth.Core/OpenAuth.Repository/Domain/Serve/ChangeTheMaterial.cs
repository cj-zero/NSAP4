using OpenAuth.Repository.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace OpenAuth.Repository.Domain.Serve
{
    /// <summary>
	/// 更换物料型号及数量表
	/// </summary>
    [Table("ChangeTheMaterial")]
    public class ChangeTheMaterial : Entity
    {
        /// <summary>
        /// 完工报告Id
        /// </summary>
        [Description("完工报告Id")]
        [Browsable(false)]
        public string CompletionReportId { get; set; }
        /// <summary>
        /// 物料
        /// </summary>
        [Description("物料")]
        public string Material { get; set; }
        /// <summary>
        /// 数量
        /// </summary>
        [Description("数量")]
        public int? Count { get; set; }
    }
}
