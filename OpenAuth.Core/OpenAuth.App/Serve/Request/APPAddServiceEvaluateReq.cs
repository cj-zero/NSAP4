using AutoMapper.Configuration.Annotations;
using Infrastructure.AutoMapper;
using OpenAuth.Repository.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Request
{
    [AutoMapTo(typeof(ServiceEvaluate), false)]
    public class APPAddServiceEvaluateReq
    {
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
        /// 回访角色 2.呼叫中心 3.业务员
        /// </summary>
        public int? EvaluateType { get; set; }
        /// <summary>
        /// 技术员评价
        /// </summary>
        [Ignore]
        public List<TechnicianEvaluate> TechnicianEvaluates { get; set; }
    }

    public class TechnicianEvaluate
    {

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
    }
}
