using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Serve.Request
{
    public class AddProblemOrSolutionReq
    {
        /// <summary>
        /// 当前登陆者App用户Id
        /// </summary>
        public int AppUserId { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 类型 1问题描述 2解决方案
        /// </summary>
        public int Type { get; set; }
    }
}
