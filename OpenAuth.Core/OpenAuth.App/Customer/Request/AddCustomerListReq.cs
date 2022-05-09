using System;
using System.Collections.Generic;
using System.Text;
using AutoMapper;
using Infrastructure.AutoMapper;
using OpenAuth.Repository.Domain;

namespace OpenAuth.App.Customer.Request
{
    [AutoMapTo(typeof(SpecialCustomer))]
    public class AddCustomerListReq
    {
        /// <summary>
        /// 客户编码;客户编码
        /// </summary>
        public string CustomerNo { get; set; }

        /// <summary>
        /// 客户名称
        /// </summary>
        public string CustomerName { get; set; }

        /// <summary>
        /// 业务员id
        /// </summary>
        public string SalerId { get; set; }

        /// <summary>
        /// 业务员名称
        /// </summary>
        public string SalerName { get; set; }

        /// <summary>
        /// 部门id
        /// </summary>
        public string DepartmentId { get; set; }

        /// <summary>
        /// 部门名称
        /// </summary>
        public string DepartmentName { get; set; }

        /// <summary>
        /// 类型：1白名单，0：黑名单
        /// </summary>
        public int Type { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }
    }
}
