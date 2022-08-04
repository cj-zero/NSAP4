using OpenAuth.Repository.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace OpenAuth.Repository.Domain.Hr
{

    /// <summary>
    /// 专题表
    /// </summary>
    [Table("classroom_subject")]
    public class classroom_subject : BaseEntity<int>
    {
        /// <summary>
        /// 专题名字
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 浏览次数
        /// </summary>
        public int ViewNumbers { get; set; }
        /// <summary>
        /// 专题状态     0=关闭  1=开放
        /// </summary>
        public int State { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }
        /// <summary>
        /// 排序
        /// </summary>
        public int Sort { get; set; }
        /// <summary>
        /// 创建人
        /// </summary>
        public string CreateUser { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public override void GenerateDefaultKeyVal()
        {
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override bool KeyIsNull()
        {
            return Id == 0;
        }
    }

    public class classroom_subject_dto : classroom_subject
    {
        /// <summary>
        /// 进度
        /// </summary>
        public decimal? Schedule { get; set; }
        /// <summary>
        /// 是否完成
        /// </summary>
        public bool IsComplete { get; set; }
        
    }
}