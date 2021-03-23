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
	/// 
	/// </summary>
    [Table("serviceevaluate")]
    public partial class ServiceEvaluate : BaseEntity<long>
    {
        public ServiceEvaluate()
        {
          this.CustomerId= string.Empty;
          this.Cutomer= string.Empty;
          this.Contact= string.Empty;
          this.CaontactTel= string.Empty;
          this.Technician= string.Empty;
          this.TechnicianId= string.Empty;
          this.Comment= string.Empty;
          this.VisitPeopleId= string.Empty;
          this.VisitPeople= string.Empty;
          this.CommentDate= DateTime.Now;
          this.CreateTime= DateTime.Now;
          this.CreateUserId= string.Empty;
          this.CreateUserName= string.Empty;
        }


        /// <summary>
        /// 服务单Id
        /// </summary>
        [Description("服务单Id")]
        public int? ServiceOrderId { get; set; }
        /// <summary>
        /// 客户代码
        /// </summary>
        [Description("客户代码")]
        public string CustomerId { get; set; }
        /// <summary>
        /// 客户名称
        /// </summary>
        [Description("客户名称")]
        public string Cutomer { get; set; }
        /// <summary>
        /// 联系人
        /// </summary>
        [Description("联系人")]
        public string Contact { get; set; }
        /// <summary>
        /// 联系电话
        /// </summary>
        [Description("联系电话")]
        public string CaontactTel { get; set; }
        /// <summary>
        /// 技术员
        /// </summary>
        [Description("技术员")]
        public string Technician { get; set; }
        /// <summary>
        /// 技术员NSAPId
        /// </summary>
        [Description("技术员NSAPId")]
        public string TechnicianId { get; set; }
        /// <summary>
        /// 技术员APPId
        /// </summary>
        [Description("技术员APPId")]
        public int? TechnicianAppId { get; set; }
        /// <summary>
        /// 响应速度打分
        /// </summary>
        [Description("响应速度打分")]
        public int? ResponseSpeed { get; set; }
        /// <summary>
        /// 方案有效性打分
        /// </summary>
        [Description("方案有效性打分")]
        public int? SchemeEffectiveness { get; set; }
        /// <summary>
        /// 服务态度打分
        /// </summary>
        [Description("服务态度打分")]
        public int? ServiceAttitude { get; set; }
        /// <summary>
        /// 产品质量打分
        /// </summary>
        [Description("产品质量打分")]
        public int? ProductQuality { get; set; }
        /// <summary>
        /// 服务价格打分
        /// </summary>
        [Description("服务价格打分")]
        public int? ServicePrice { get; set; }
        /// <summary>
        /// 客户建议或意见
        /// </summary>
        [Description("客户建议或意见")]
        public string Comment { get; set; }
        /// <summary>
        /// 回访人Id
        /// </summary>
        [Description("回访人Id")]
        [Browsable(false)]
        public string VisitPeopleId { get; set; }
        /// <summary>
        /// 回访人名字
        /// </summary>
        [Description("回访人名字")]
        public string VisitPeople { get; set; }
        /// <summary>
        /// 评价日期
        /// </summary>
        [Description("评价日期")]
        public System.DateTime? CommentDate { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        [Description("创建时间")]
        public System.DateTime? CreateTime { get; set; }
        /// <summary>
        /// 创建人
        /// </summary>
        [Description("创建人")]
        [Browsable(false)]
        public string CreateUserId { get; set; }
        /// <summary>
        /// 创建人名称
        /// </summary>
        [Description("创建人名称")]
        public string CreateUserName { get; set; }

        /// <summary>
        /// 评价人角色1.客户 2.呼叫中心 3.业务员
        /// </summary>
        [Description("评价人角色")]
        public int? EvaluateType { get; set; }
        

        public override void GenerateDefaultKeyVal()
        {
            //throw new NotImplementedException();
        }

        public override bool KeyIsNull()
        {
            return Id == 0;
        }
    }
}