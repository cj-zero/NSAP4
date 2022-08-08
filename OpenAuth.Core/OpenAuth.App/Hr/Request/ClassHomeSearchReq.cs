using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Hr
{

    /// <summary>
    /// 课堂首页模糊查询
    /// </summary>
    public class ClassHomeSearchReq
    {
        /// <summary>
        /// 用户id
        /// </summary>
        public int appUserId { get; set; }

        /// <summary>
        /// 类型 1-必修必答 2-专题讲堂 3-课程直播 
        /// </summary>
        public int type { get; set; }

        /// <summary>
        /// 关键词
        /// </summary>
        public string  key { get; set; }

        /// <summary>
        /// 第几页
        /// </summary>
        public int pageIndex { get; set; }
        /// <summary>
        /// 每页的行数
        /// </summary>
        public int pageSize { get; set; }
    }
}
