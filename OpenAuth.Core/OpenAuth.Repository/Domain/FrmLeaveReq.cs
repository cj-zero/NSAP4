//------------------------------------------------------------------------------
// <autogenerated>
//     This code was generated by a CodeSmith Template.
//
//     DO NOT MODIFY contents of this file. Changes to this
//     file will be lost if the code is regenerated.
//     Author:Yubao Li
// </autogenerated>
//------------------------------------------------------------------------------
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using OpenAuth.Repository.Core;

namespace OpenAuth.Repository.Domain
{
    /// <summary>
	/// 模拟一个自定页面的表单，该数据会关联到流程实例FrmData，可用于复杂页面的设计及后期的数据分析
	/// </summary>
      [Table("FrmLeaveReq")]
    public partial class FrmLeaveReq : Entity
    {
        public FrmLeaveReq()
        {
          this.UserName= string.Empty;
          this.RequestType= string.Empty;
          this.StartDate= DateTime.Now;
          this.StartTime= DateTime.Now;
          this.EndDate= DateTime.Now;
          this.EndTime= DateTime.Now;
          this.RequestComment= string.Empty;
          this.Attachment= string.Empty;
          this.CreateDate= DateTime.Now;
          this.CreateUserId= string.Empty;
          this.CreateUserName= string.Empty;
            this.FlowInstanceId = string.Empty;
        }

        /// <summary>
	    /// 请假人姓名
	    /// </summary>
         [Description("请假人姓名")]
        public string UserName { get; set; }
        /// <summary>
	    /// 请假分类，病假，事假，公休等
	    /// </summary>
         [Description("请假分类，病假，事假，公休等")]
        public string RequestType { get; set; }
        /// <summary>
	    /// 开始日期
	    /// </summary>
         [Description("开始日期")]
        public System.DateTime StartDate { get; set; }
        /// <summary>
	    /// 开始时间
	    /// </summary>
         [Description("开始时间")]
        public System.DateTime? StartTime { get; set; }
        /// <summary>
	    /// 结束日期
	    /// </summary>
         [Description("结束日期")]
        public System.DateTime EndDate { get; set; }
        /// <summary>
	    /// 结束时间
	    /// </summary>
         [Description("结束时间")]
        public System.DateTime? EndTime { get; set; }
        /// <summary>
	    /// 请假说明
	    /// </summary>
         [Description("请假说明")]
        public string RequestComment { get; set; }
        /// <summary>
	    /// 附件，用于提交病假证据等
	    /// </summary>
         [Description("附件，用于提交病假证据等")]
        public string Attachment { get; set; }
        /// <summary>
	    /// 创建时间
	    /// </summary>
         [Description("创建时间")]
        public System.DateTime CreateDate { get; set; }
        /// <summary>
	    /// 创建用户主键
	    /// </summary>
         [Description("创建用户主键")]
        public string CreateUserId { get; set; }
        /// <summary>
	    /// 创建用户
	    /// </summary>
         [Description("创建用户")]
        public string CreateUserName { get; set; }
        

        /// <summary> 
        ///    所属流程实例ID
        /// </summary>
        public string FlowInstanceId { get; set; }

    }
}