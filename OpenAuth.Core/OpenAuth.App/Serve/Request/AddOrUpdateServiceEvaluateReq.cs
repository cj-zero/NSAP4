//------------------------------------------------------------------------------
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
using Infrastructure.AutoMapper;
using OpenAuth.Repository.Core;
using OpenAuth.Repository.Domain;

namespace OpenAuth.App.Request
{
    /// <summary>
	/// 
	/// </summary>
    [Table("serviceevaluate")]
    [AutoMapTo(typeof(ServiceEvaluate))]
    public partial class AddOrUpdateServiceEvaluateReq 
    {

        /// <summary>
        /// Id
        /// </summary>
        public long Id { get; set; }
        /// <summary>
        /// 服务单Id
        /// </summary>
        public int? ServiceOrderId { get; set; }
        /// <summary>
        /// 客户代码
        /// </summary>
        public string CustomerId { get; set; }
        /// <summary>
        /// 客户名称
        /// </summary>
        public string Cutomer { get; set; }
        /// <summary>
        /// 联系人
        /// </summary>
        public string Contact { get; set; }
        /// <summary>
        /// 联系电话
        /// </summary>
        public string CaontactTel { get; set; }
        /// <summary>
        /// 技术员
        /// </summary>
        public string Technician { get; set; }
        /// <summary>
        /// 技术员NSAPId
        /// </summary>
        public string TechnicianId { get; set; }
        /// <summary>
        /// 技术员APPId
        /// </summary>
        public int? TechnicianAppId { get; set; }
        /// <summary>
        /// 响应速度打分
        /// </summary>
        public int? ResponseSpeed { get; set; }
        /// <summary>
        /// 方案有效性打分
        /// </summary>
        public int? SchemeEffectiveness { get; set; }
        /// <summary>
        /// 服务态度打分
        /// </summary>
        public int? ServiceAttitude { get; set; }
        /// <summary>
        /// 产品质量打分
        /// </summary>
        public int? ProductQuality { get; set; }
        /// <summary>
        /// 服务价格打分
        /// </summary>
        public int? ServicePrice { get; set; }
        /// <summary>
        /// 客户建议或意见
        /// </summary>
        public string Comment { get; set; }
        /// <summary>
        /// 回访人Id
        /// </summary>
        public string VisitPeopleId { get; set; }
        /// <summary>
        /// 回访人名字
        /// </summary>
        public string VisitPeople { get; set; }
        /// <summary>
        /// 评价日期
        /// </summary>
        public System.DateTime? CommentDate { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public System.DateTime? CreateTime { get; set; }
        /// <summary>
        /// 创建人
        /// </summary>
        public string CreateUserId { get; set; }
        /// <summary>
        /// 创建人名称
        /// </summary>
        public string CreateUserName { get; set; }
    }
}