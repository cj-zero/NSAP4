using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Nwcali.Request
{
    public class DeviceCheckTaskReq
    {
        public string EdgeGuid { get; set; }
        /// <summary>
        /// 上位机guid
        /// </summary>
        public string SrvGuid { get; set; }
        /// <summary>
        /// 中位机编号
        /// </summary>
        public int DevUid { get; set; }
        /// <summary>
        /// 下位机单元id
        /// </summary>
        public int UnitId { get; set; }
        public int ChlId { get; set; }
        public long TestId { get; set; }
        public  List<object> CheckItems{get;set;}
    }
}
