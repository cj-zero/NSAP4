﻿//------------------------------------------------------------------------------
// <autogenerated>
//     This code was generated by a CodeSmith Template.
//
//     DO NOT MODIFY contents of this file. Changes to this
//     file will be lost if the code is regenerated.
//     Author:Yubao Li
// </autogenerated>
//------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using OpenAuth.Repository.Core;

namespace OpenAuth.App.Request
{
    /// <summary>
	/// 
	/// </summary>
    [Table("attendanceclock")]
    public partial class AddOrUpdateAttendanceClockReq 
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
        
         //todo:添加自己的请求字段
        public List<AttendanceClockPictureReq> AttendanceClockPictures { get; set; }
    }
}