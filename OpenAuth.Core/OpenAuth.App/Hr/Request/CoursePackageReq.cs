using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Hr
{
    /// <summary>
    /// 课程包
    /// </summary>
    public class CoursePackageReq
    {
        public int id { get; set; }
        /// <summary>
        /// 课程包名称
        /// </summary>
        public string name { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string remark { get; set; }
    }
}
