using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using OpenAuth.Repository.Core;

namespace OpenAuth.Repository.Domain.Serve
{
    /// <summary>
    /// 服务未完工原因历史表
    /// </summary>
    [Table("serviceuncompletedreasonhistory")]
    public class ServiceUnCompletedReasonHistory : Entity
    {
        /// <summary>
        /// 自增Id
        /// </summary>
        [Key]
        public string Id { get; set; }
        /// <summary>
        /// 服务单Id
        /// </summary>
        public int ServiceOrderId { get; set; }
        /// <summary>
        /// 技术员Id
        /// </summary>
        public string FroTechnicianId { get; set; }
        /// <summary>
        /// 技术员名称
        /// </summary>
        public string FroTechnicianName { get; set; }
        /// <summary>
        /// 内容
        /// </summary>
        public string Content { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }
        /// <summary>
        /// 创建人
        /// </summary>
        public string CreateUserId { get; set; }
    }

    /// <summary>
    /// 服务未完工原因明细表
    /// </summary>
    [Table("serviceuncompletedreasondetail")]
    public class ServiceUnCompletedReasonDetail : Entity
    {
        /// <summary>
        /// 自增Id
        /// </summary>
        [Key]
        public string Id { get; set; }

        /// <summary>
        /// 未完工原因历史Id
        /// </summary>
        public string HistoryId { get; set; }

        /// <summary>
        /// 服务单Id
        /// </summary>
        public int ServiceOrderId { get; set; }
        /// <summary>
        /// 未完工原因Id
        /// </summary>
        public string UnCompletedReasonId { get; set; }
        /// <summary>
        /// 未完工原因
        /// </summary>
        public string UnCompletedReasonName { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }
        /// <summary>
        /// 创建人
        /// </summary>
        public string CreateUserId { get; set; }
    }
}
