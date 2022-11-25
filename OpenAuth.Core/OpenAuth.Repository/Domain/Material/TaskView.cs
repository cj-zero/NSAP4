/*
 * @author : wangying
 * @date : 2022-8-9
 * @desc : 工程部物料设计
 */
using OpenAuth.Repository.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace OpenAuth.Repository.Domain
{
    /// <summary>
    /// 工程部物料设计
    /// </summary>
    [Table("taskview")]
    public class TaskView : BaseEntity<int>
    {
        /// <summary>
        /// 零件号
        /// </summary>
        public string Number { get; set; }
        /// <summary>
        /// 考勤月份
        /// </summary>
        public string Month { get; set; }
        /// <summary>
        /// 创建人
        /// </summary>
        public string CreateUser { get; set; }
        /// <summary>
        /// 创建日期
        /// </summary>
        public DateTime? CreateDate { get; set; }
        /// <summary>
        /// 编辑人
        /// </summary>
        public string UpdateUser { get; set; }
        /// <summary>
        /// 编辑日期
        /// </summary>
        public DateTime? UpdateDate { get; set; }

        public override void GenerateDefaultKeyVal()
        {
        }

        public override bool KeyIsNull()
        {
            return Id == 0;
        }
    }
}
