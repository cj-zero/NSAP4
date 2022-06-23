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
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using OpenAuth.Repository.Core;

namespace OpenAuth.App.Request
{
    /// <summary>
	/// 
	/// </summary>
    [Table("stepversion")]
    public partial class AddOrUpdateStepVersionReq
    {

        /// <summary>
        /// 主键Id
        /// </summary>
        public long Id { get; set; }
        /// <summary>
        /// 工步名称
        /// </summary>
        public string StepName { get; set; }
        /// <summary>
        /// 系列名称
        /// </summary>
        public string SeriesName { get; set; }
        /// <summary>
        /// 工步量程
        /// </summary>
        public string StepVersionName { get; set; }
        /// <summary>
        /// 工步文件路径
        /// </summary>
        public string FilePath { get; set; }
        /// <summary>
        /// 工步文件名称
        /// </summary>
        public string FileName { get; set; }
        /// <summary>
        /// 备注信息
        /// </summary>
        public string Remark { get; set; }
        /// <summary>
        /// 创建人
        /// </summary>
        public string CreateUser { get; set; }
        /// <summary>
        /// 创建人
        /// </summary>
        public string CreateUserId { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public System.DateTime CreateTime { get; set; }
        /// <summary>
        /// 修改时间
        /// </summary>
        public System.DateTime UpdateTime { get; set; }
        /// <summary>
        /// 排序降序
        /// </summary>
        public int Sorts { get; set; }
        /// <summary>
        /// 是否启用
        /// </summary>
        public bool Status { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string FilePath2 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string FileName2 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Remark2 { get; set; }
        /// <summary>
        /// 优先启动（1：工步1  2:工步2）
        /// </summary>
        public int FirstStart { get; set; }
        /// <summary>
        /// 电压
        /// </summary>
        public decimal Voltage { get; set; }
        /// <summary>
        /// 电流
        /// </summary>
        public decimal Current { get; set; }
    }
}
