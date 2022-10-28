using OpenAuth.Repository.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Serve.Response
{
    /// <summary>
    /// 
    /// </summary>
    public class ServiceOrderLogResp
    {
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime? CreateTime { get; set; }
        /// <summary>
        /// 操作人
        /// </summary>
        public string CreateUserName { get; set; }
        /// <summary>
        /// 内容
        /// </summary>

        public string Action { get; set; }
    }

    public class ServiceWorkOrderList
    {
        /// <summary>
        /// 服务单Id
        /// </summary>
        public int ServiceOrderId { get; set; }
        /// <summary>
        /// 客户代码
        /// </summary>
        public string CustomerId { get; set; }
        /// <summary>
        /// 客户名称
        /// </summary>
        public string CustomerName { get; set; }
        /// <summary>
        /// 终端客户代码
        /// </summary>
        public string TerminalCustomerId { get; set; }
        /// <summary>
        /// 终端客户
        /// </summary>
        public string TerminalCustomer { get; set; }
        /// <summary>
        /// 接单人姓名
        /// </summary>
        public string RecepUserId { get; set; }
        /// <summary>
        /// 接单人姓名
        /// </summary>
        public string RecepUserName { get; set; }
        /// <summary>
        /// 接单人部门
        /// </summary>
        public string RecepUserDept { get; set; }
        /// <summary>
        /// 联系人
        /// </summary>
        public string Contacter { get; set; }
        /// <summary>
        /// 联系人电话
        /// </summary>
        public string ContactTel { get; set; }
        /// <summary>
        /// 最新联系人
        /// </summary>
        public string NewestContacter { get; set; }
        /// <summary>
        /// 最新联系人电话号码
        /// </summary>
        public string NewestContactTel { get; set; }
        /// <summary>
        /// 主管名字
        /// </summary>
        public string Supervisor { get; set; }
        /// <summary>
        /// 主管名字
        /// </summary>
        public string SupervisorId { get; set; }
        /// <summary>
        /// 主管部门
        /// </summary>
        public string SuperVisorDept { get; set; }
        /// <summary>
        /// 销售名字
        /// </summary>
        public string SalesMan { get; set; }
        /// <summary>
        /// 销售名字
        /// </summary>
        public string SalesManId { get; set; }
        /// <summary>
        /// 销售部门
        /// </summary>
        public string SalesManDept { get; set; }
        /// <summary>
        /// SAP服务单ID
        /// </summary>
        public int? U_SAP_ID { get; set; }
        /// <summary>
        /// 归属部门
        /// </summary>
        public int? VestInOrg { get; set; }
        /// <summary>
        /// 服务单状态
        /// </summary>
        public int ServiceStatus { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime? ServiceCreateTime { get; set; }
        /// <summary>
        /// 是否允许服务
        /// </summary>
        public int AllowOrNot { get; set; }
        /// <summary>
        /// 撤回备注
        /// </summary>
        public string Remark { get; set; }
        /// <summary>
        /// 未完工原因
        /// </summary>
        public string UnCompletedReason { get; set; }
        public int? FromId { get; set; }
        public List<ServiceWorkOrder> ServiceWorkOrders { get; set; }
        public List<string> MaterialTypes { get; set; }

        /// <summary>
        /// 是否承包
        /// </summary>
        public int? IsContracting { get; set; }
        

    }
}
