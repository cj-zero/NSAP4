using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Hr
{
    /// <summary>
    /// 题目
    /// </summary>
    public class SubjectResp
    {
        /// <summary>
        /// 题目id
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// 难度
        /// </summary>
        public int difficulty_level { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public DateTime created_time { get; set; }
        /// <summary>
        /// 类型：1-单选 2-多选 3-填空
        /// </summary>
        public int type { get; set; }
        /// <summary>
        /// 图片
        /// </summary>
        public string image_url { get; set; }
        /// <summary>
        /// 视频
        /// </summary>
        public string video_url { get; set; }
        /// <summary>
        /// 审核状态:0-待审核,1-同意,2-拒绝
        /// </summary>
        public int audit_status { get; set; }
        /// <summary>
        /// 删除标识
        /// </summary>
        public bool is_delete { get; set; }
        /// <summary>
        /// 标签
        /// </summary>
        public string labeName { get; set; }
        /// <summary>
        /// 选项
        /// </summary>
        public string answer_options { get; set; }
        /// <summary>
        /// 正确答案，多个答案用 ','隔开
        /// </summary>
        public string standard_answer { get; set; }
        /// <summary>
        /// 本题分数
        /// </summary>
        public int score { get; set; }
        /// <summary>
        /// 题目
        /// </summary>
        public string content { get; set; }
    }
}
