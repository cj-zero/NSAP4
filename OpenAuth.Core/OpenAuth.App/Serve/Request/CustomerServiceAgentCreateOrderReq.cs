using AutoMapper.Configuration.Annotations;
using Infrastructure.AutoMapper;
using OpenAuth.App.Request;
using OpenAuth.Repository.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Request
{
    [AutoMapTo(typeof(ServiceOrder))]
    public class CustomerServiceAgentCreateOrderReq
    {
        //public int Id { get; set; }
        /// <summary>
        /// 客户代码
        /// </summary>
        public string CustomerId { get; set; }
        /// <summary>
        /// 客户名称
        /// </summary>
        public string CustomerName { get; set; }
        /// <summary>
        /// 联系人
        /// </summary>
        public string Contacter { get; set; }
        /// <summary>
        /// 联系人电话
        /// </summary>
        public string ContactTel { get; set; }
        /// <summary>
        /// 主管名字
        /// </summary>
        public string Supervisor { get; set; }
        /// <summary>
        /// 主管用户Id
        /// </summary>
        public string SupervisorId { get; set; }
        /// <summary>
        /// 销售名字
        /// </summary>
        public string SalesMan { get; set; }
        /// <summary>
        /// 销售用户Id
        /// </summary>
        public string SalesManId { get; set; }
        /// <summary>
        /// 最新联系人
        /// </summary>
        public string NewestContacter { get; set; }
        /// <summary>
        /// 最新联系人电话号码
        /// </summary>
        public string NewestContactTel { get; set; }

        /// <summary>
        /// 终端客户代码
        /// </summary>
        public string TerminalCustomerId { get; set; }
        /// <summary>
        /// 终端客户
        /// </summary>
        public string TerminalCustomer { get; set; }

        /// <summary>
        /// 地址标识
        /// </summary>
        public string AddressDesignator { get; set; }
        /// <summary>
        /// 接单人用户Id
        /// </summary>
        public string RecepUserId { get; set; }
        /// <summary>
        /// 接单人姓名
        /// </summary>
        public string RecepUserName { get; set; }
        /// <summary>
        /// 详细地址
        /// </summary>
        public string Address { get; set; }
        /// <summary>
        /// 省
        /// </summary>
        public string Province { get; set; }
        /// <summary>
        /// 市
        /// </summary>
        public string City { get; set; }
        /// <summary>
        /// 区
        /// </summary>
        public string Area { get; set; }
        /// <summary>
        /// 地区
        /// </summary>
        public string Addr { get; set; }
        /// <summary>
        /// 经度
        /// </summary>
        public decimal? Longitude { get; set; }
        /// <summary>
        /// 纬度
        /// </summary>
        public decimal? Latitude { get; set; }
        /// <summary>
        /// 呼叫来源  1-电话 2-APP 
        /// </summary>
        //[Browsable(false)]
        public int? FromId { get; set; }

        /// <summary>
        /// 服务单关联的工单
        /// </summary>
        public virtual List<AddServiceWorkOrderReq> ServiceWorkOrders { get; set; }

        [Ignore]
        public virtual List<FileBind> Pictures { get; set; }

        public virtual List<ServiceOrderSerial> ServiceOrderSNs { get; set; }
    }
}
