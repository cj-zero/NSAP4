using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Response
{
    public class AttendanceClockDetailsResp
    {
        /// <summary>
        /// 
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string UserId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Org { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string OrgId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public System.DateTime? ClockDate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public System.TimeSpan? ClockTime { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public decimal? Longitude { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public decimal? Latitude { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Location { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string SpecificLocation { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string VisitTo { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Remark { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string PhoneId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Ip { get; set; }

        /// <summary>
        /// 图片文件列表
        /// </summary>
        public virtual List<UploadFileResp> Files { get; set; }
    }

}
