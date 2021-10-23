using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Meeting.ModelDto
{
    public class MeetingUserListDto
    {
        public int id { get; set; }
        /// <summary>
        /// 报名人
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 报名时间
        /// </summary>

        public DateTime CreateTime { get; set; }
    }
}
