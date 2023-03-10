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
        /// 服务内容
        /// </summary>
        public string Services { get; set; }
        /// <summary>
        /// 联系人
        /// </summary>
        public string NewestContacter { get; set; }
        /// <summary>
        /// 联系人电话
        /// </summary>
        public string NewestContactTel { get; set; }
        /// <summary>
        /// App用户Id
        /// </summary>
        public int? AppUserId { get; set; }
        /// <summary>
        /// App技术主管Id
        /// </summary>
        public int? ManagerId { get; set; }
        /// <summary>
        /// 接单人用户Id
        /// </summary>
        public string RecepUserId { get; set; }
        /// <summary>
        /// 接单人姓名
        /// </summary>
        public string RecepUserName { get; set; }
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
        /// 详细地址
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
        /// 问题类型Id
        /// </summary>
        public string ProblemTypeId { get; set; }
        /// <summary>
        /// 问题类型名称
        /// </summary>
        public string ProblemTypeName { get; set; }
        ///// <summary>
        ///// 服务单关联的工单
        ///// </summary>
        //public virtual List<AddServiceWorkOrderReq> ServiceWorkOrders { get; set; }

        /// <summary>
        /// 服务单关联的图片
        /// </summary>
        public virtual List<FileBind> Pictures { get; set; }

        /// <summary>
        /// 序列号物料编码
        /// </summary>
        public virtual List<ServiceOrderSerial> ServiceOrderSNs { get; set; }
    }
}
