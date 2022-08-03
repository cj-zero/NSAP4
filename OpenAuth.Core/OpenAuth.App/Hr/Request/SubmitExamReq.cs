using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Hr
{
    /// <summary>
    /// 
    /// </summary>
    public class SubmitExamReq
    {
        /// <summary>
        /// 考卷id
        /// </summary>
        public int ExaminationId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public List<SubjectList> subjectList { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class SubjectList
    {
        /// <summary>
        /// 本试卷题目id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 类型：1-单选 2-多选 3-填空
        /// </summary>
        public int type { get; set; }

        /// <summary>
        /// 选择选项ID，多选题用英文逗号隔开
        /// </summary>
        public string optionIds { get; set; }
    }
}
