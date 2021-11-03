using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Meeting.ModelDto
{
    public class ProcessStepsDto
    {
        /// <summary>
        /// 草稿Id
        /// </summary>
        public int DraftId { get; set; }
        /// <summary>
        /// 日志
        /// </summary>
        public string Log { get; set; }
        /// <summary>
        /// 操作人
        /// </summary>
        public string CreateUser { get; set; }
        /// <summary>
        /// 操作时间
        /// </summary>
        public DateTime CreateTime { get; set; }
    }
}
