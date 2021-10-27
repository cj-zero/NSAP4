using AutoMapper;
using AutoMapper.Configuration.Annotations;
using OpenAuth.App.Request;
using OpenAuth.Repository.Domain.Serve;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Meeting.ModelDto
{
    [AutoMap(typeof(Repository.Domain.Serve.Meeting))]
    public class QueryListDto
    {
        /// <summary>
        /// 展会Id
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// 展会名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 展会开始时间
        /// </summary>
        public string  StartTime { get; set; }
        /// <summary>
        /// 展会结束时间
        /// </summary>
        public string EndTime { get; set; }
        /// <summary>
        /// 地址
        /// </summary>
        public string Address { get; set; }
        /// <summary>
        /// 有无晚宴
        /// </summary>
        public bool IsDinner { get; set; }

        /// <summary>
        /// 申请部门
        /// </summary>
        public string ApplyDempName { get; set; }
        /// <summary>
        /// 申请人
        /// </summary>
        public string ApplyUser { get; set; }
        /// <summary>
        /// 跟进人
        /// </summary>
        public string FollowPerson { get; set; }
        /// <summary>
        /// 状态
        /// </summary>
        public int Status { get; set; }
        /// <summary>
        /// 报名人数 
        /// </summary>
        [Ignore]
        public int number { get; set; }
        /// <summary>
        /// 0：国内，1：国外
        /// </summary>
        public int AddressType { get; set; }
    }
}
