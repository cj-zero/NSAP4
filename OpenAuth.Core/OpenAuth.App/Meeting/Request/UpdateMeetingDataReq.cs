using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Meeting.Request
{
    public class UpdateMeetingDataReq : AddMeetingDataReq
    {
        /// <summary>
        /// 会议ID
        /// </summary>
        public int Id { get; set; }
    }
}
