using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using OpenAuth.Repository.Core;

namespace OpenAuth.Repository.Domain
{
    /// <summary>
    /// 请求日志表
    /// </summary>
    [Table("RequestActionLog")]
    public class RequestActionLog : Entity
    {
        /// <summary>
        /// 自增主键
        /// </summary>
        [Key]
        [Description("自增主键")]
        public int Id { get; set; }

        /// <summary>
        /// 请求的action名
        /// </summary>
        [Description("请求的action名")]
        public string ActionName { get; set; }

        /// <summary>
        /// 请求时传过来的参数
        /// </summary>
        /// 
        [Description("请求时传过来的参数")]
        public string Parameter { get; set; }

        /// <summary>
        /// 请求用户
        /// </summary>
        /// 
        [Description("请求用户")]
        public string RequestUser { get; set; }

        /// <summary>
        /// 请求api的返回结果
        /// </summary>
        /// 
        [Description("请求api的返回结果")]
        public string ApiResult { get; set; }

        /// <summary>
        /// 请求时间
        /// </summary>
        /// 
        [Description("请求时间")]
        public DateTime RequestTime { get; set; }
    }
}
