using OpenAuth.App.Meeting.ModelDto;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Clue.Request
{
    /// <summary>
    /// 更新日程model
    /// </summary>
    public class UpdateClueScheduleReq:AddClueScheduleReq
    {
        /// <summary>
        /// Id
        /// </summary>
        public int Id { get; set; }

    }
}
