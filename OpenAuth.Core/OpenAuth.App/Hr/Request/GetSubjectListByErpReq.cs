using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Hr.Request
{
    /// <summary>
    /// 专题列表请求
    /// </summary>
    public class GetSubjectListByErpReq
    {
        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 创建人
        /// </summary>
        public string CreateUser { get; set; }

        /// <summary>
        /// 课程状态  0=关闭  1=开放
        /// </summary>
        public int? State { get; set; }

        /// <summary>
        /// 时间
        /// </summary>
        public DateTime? StartTime { get; set; }

        /// <summary>
        /// 时间
        /// </summary>
        public DateTime? EndTime { get; set; }

        public int pageIndex { get; set; }
        public int pageSize { get; set; }
    }
    public class GetSubjectCourseListByErpReq : GetSubjectListByErpReq
    {
        public int subjectId { get; set; }
    }

}



