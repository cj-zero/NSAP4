using Castle.DynamicProxy.Generators;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace OpenAuth.App.Request
{
    public class QueryServiceOrderDetailReq
    {
        /// <summary>
        /// 服务单Id
        /// </summary>
        [Required]
        public int ServiceOrderId { get; set; }
        /// <summary>
        /// 经度
        /// </summary>
        public decimal Longitude { get; set; }
        /// <summary>
        /// 纬度
        /// </summary>
        public decimal Latitude { get; set; }

        /// <summary>
        /// 当前技术员Id
        /// </summary>
        public int CurrentUserId { get; set; }

        /// <summary>
        /// 服务单类型 0未接单  1已接单
        /// </summary>
        [Required]
        public int Type { get; set; }
    }
}
