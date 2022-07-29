using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Hr
{
    /// <summary>
    /// 新增或编辑课程
    /// </summary>
    public class AddOrEditCourseReq
    {
        /// <summary>
        /// 课程id
        /// </summary>
        public int id { get; set; }
        /// <summary>
        /// 课程名字
        /// </summary>
        public string name { get; set; }
        /// <summary>
        /// 课程来源(1:主管推课  2:职前  3:入职  4:晋升  5:转正 6:变动)
        /// </summary>
        public int source { get; set; }
        /// <summary>
        /// 课程学习周期(天)
        /// </summary>
        public int learningCycle { get; set; }
    }
}
