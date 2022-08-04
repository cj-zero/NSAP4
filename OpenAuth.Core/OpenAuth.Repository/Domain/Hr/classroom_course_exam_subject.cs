using OpenAuth.Repository.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace OpenAuth.Repository.Domain
{
    /// <summary>
    /// 
    /// </summary>
    [Table("classroom_course_exam_subject")]
    public class classroom_course_exam_subject : BaseEntity<int>
    {
        /// <summary>
        /// 试卷id
        /// </summary>
        public int ExaminationId { get; set; }
        /// <summary>
        /// 类型：1-单选 2-多选 3-填空
        /// </summary>
        public int Type { get; set; }
        /// <summary>
        /// 题目内容
        /// </summary>
        public string Content { get; set; }
        /// <summary>
        ///  答案json数组 
        /// </summary>
        public string AnswerOptions { get; set; }
        /// <summary>
        /// 答案
        /// </summary>
        public string StandardAnswer { get; set; }
        /// <summary>
        /// 分值
        /// </summary>
        public int Score { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }
        /// <summary>
        /// 修改时间
        /// </summary>
        public DateTime ModifyTime { get; set; }
        /// <summary>
        /// 答题状态: 0-未答 1-答题正确  2-答题错误
        /// </summary>
        public int AnswerStatus { get; set; }
        /// <summary>
        /// 视频地址
        /// </summary>
        public string VideoUrl { get; set; }
        /// <summary>
        /// 用户填空题答案
        /// </summary>
        public string AnswerContent { get; set; }
        /// <summary>
        /// 图片地址
        /// </summary>
        public string ImageUrl { get; set; }
        /// <summary>
        /// 题目id
        /// </summary>
        public int SubjectId { get; set; }


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
}
