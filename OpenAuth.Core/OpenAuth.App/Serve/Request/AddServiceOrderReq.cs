using Infrastructure.AutoMapper;
using OpenAuth.Repository.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Request
{
    [AutoMapTo(typeof(ServiceOrder))]
    public class AddServiceOrderReq
    {
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
        /// 终端客户
        /// </summary>
        public string TerminalCustomer { get; set; }

        /// <summary>
        /// 接单人用户Id
        /// </summary>
        public string RecepUserId { get; set; }
        /// <summary>
        /// 接单人姓名
        /// </summary>
        public string RecepUserName { get; set; }
        /// <summary>
        /// 服务单关联的工单
        /// </summary>
        public virtual List<AddServiceWorkOrderReq> ServiceWorkOrders { get; set; }

        public virtual List<FileBind> Pictures { get; set; }

        public virtual List<ServiceOrderSerial> ServiceOrderSNs { get; set; }
    }
}
