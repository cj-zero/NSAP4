using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Nwcali.Response
{
    /// <summary>
    /// 工步内容
    /// </summary>
    public class StepContentResp
    {
        /// <summary>
        /// 工步数量
        /// </summary>
        public int stepCount { get; set; }
        /// <summary>
        /// 工步数据
        /// </summary>
        public string stepData { get; set; }
    }
}
