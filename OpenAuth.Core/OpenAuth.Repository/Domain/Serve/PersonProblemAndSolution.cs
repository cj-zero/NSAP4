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
	/// 
	/// </summary>
    [Table("personproblemandsolution")]
    public partial class PersonProblemAndSolution : Entity
    {
        public PersonProblemAndSolution()
        {
            this.Description = string.Empty;
            this.CreateTime = DateTime.Now;
        }


        /// <summary>
        /// 描述
        /// </summary>
        [Description("描述")]
        public string Description { get; set; }
        /// <summary>
        /// 类型 1问题描述 2解决方案
        /// </summary>
        [Description("类型 1问题描述 2解决方案")]
        public int? Type { get; set; }
        /// <summary>
        /// Erp用户Id
        /// </summary>
        [Description("Erp用户Id")]
        public string CreaterId { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        [Description("创建时间")]
        public System.DateTime? CreateTime { get; set; }
    }
}