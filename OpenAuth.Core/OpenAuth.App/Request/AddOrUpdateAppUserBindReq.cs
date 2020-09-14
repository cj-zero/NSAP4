using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using OpenAuth.Repository.Core;

namespace OpenAuth.App.Request
{
    /// <summary>
	/// 
	/// </summary>
    [Table("AppUserBind")]
    public partial class AddOrUpdateAppUserBindReq
    {

        /// <summary>
        /// 绑定申请Id
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// App用户Id
        /// </summary>
        public int? AppUserId { get; set; }
        /// <summary>
        /// 姓名
        /// </summary>
        public string RealName { get; set; }
        /// <summary>
        /// 客户代码
        /// </summary>
        public string CustomerCode { get; set; }
        /// <summary>
        /// 客户名称
        /// </summary>
        public string CustomerName { get; set; }
        /// <summary>
        /// 联系人
        /// </summary>
        public string Linkman { get; set; }
        /// <summary>
        /// 联系人手机号
        /// </summary>
        public string LinkmanTel { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public System.DateTime? CreateTime { get; set; }
        /// <summary>
        /// 审核状态 0待审核 1审核成功 2审核失败
        /// </summary>
        public int? AuditState { get; set; }
        /// <summary>
        /// 审核失败原因
        /// </summary>
        public string RefuseReason { get; set; }
        /// <summary>
        /// 审核操作人
        /// </summary>
        public int? Aduiter { get; set; }
        /// <summary>
        /// 审核时间
        /// </summary>
        public System.DateTime? AuditTime { get; set; }
        //todo:添加自己的请求字段
    }
}