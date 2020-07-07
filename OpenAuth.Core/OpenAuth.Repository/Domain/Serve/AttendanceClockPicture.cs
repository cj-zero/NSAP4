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

namespace OpenAuth.Repository.Domain
{
    /// <summary>
	/// 打卡记录图片表
	/// </summary>
    [Table("attendanceclockpicture")]
    public partial class AttendanceClockPicture : Entity
    {
        public AttendanceClockPicture()
        {
        }

        /// <summary>
        /// 打卡记录流水Id
        /// </summary>
        public string AttendanceClockId { get; set; }
        /// <summary>
        /// 图片Id
        /// </summary>
        [Description("图片Id")]
        public string PictureId { get; set; }
    }
}