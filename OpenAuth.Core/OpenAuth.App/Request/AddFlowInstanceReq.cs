﻿//------------------------------------------------------------------------------
// <autogenerated>
//     This code was generated by a CodeSmith Template.
//
//     DO NOT MODIFY contents of this file. Changes to this
//     file will be lost if the code is regenerated.
//     Author:Yubao Li
// </autogenerated>
//------------------------------------------------------------------------------

using Infrastructure.AutoMapper;
using OpenAuth.Repository.Domain;
using System.ComponentModel;

namespace OpenAuth.App.Request
{
    /// <summary>
	/// 创建工作流请求
	/// </summary>
    [AutoMapTo(typeof(FlowInstance))]
    public class AddFlowInstanceReq 
    {

        /// <summary>
	    /// 实例编号
	    /// </summary>
         [Description("实例编号")]
        public string Code { get; set; }
        /// <summary>
	    /// 自定义名称
	    /// </summary>
         [Description("自定义名称")]
        public string CustomName { get; set; }



        /// <summary>
	    /// 流程模板内容
	    /// </summary>
         [Description("流程模板内容")]
        public string SchemeContent { get; set; }
        /// <summary>
	    /// 流程模板ID
	    /// </summary>
        public string SchemeId { get; set; }

        /// <summary>
        /// 流程模板自定义编号
        /// </summary>
        public string SchemeCode { get; set; }


        /// <summary>
	    /// 数据库名称
	    /// </summary>
         [Description("数据库名称")]
        public string DbName { get; set; }
        /// <summary>
	    /// 表单数据
	    /// </summary>
         [Description("表单数据")]
        public string FrmData { get; set; }
        /// <summary>
	    /// 表单类型
	    /// </summary>
         [Description("表单类型")]
        public int FrmType { get; set; }
        /// <summary>
	    /// 表单中的控件属性描述
	    /// </summary>
         [Description("表单中的控件属性描述")]
        public string FrmContentData { get; set; }
        /// <summary>
	    /// 表单控件位置模板
	    /// </summary>
         [Description("表单控件位置模板")]
        public string FrmContentParse { get; set; }
        /// <summary>
	    /// 表单ID
	    /// </summary>
         [Description("表单ID")]
        public string FrmId { get; set; }
        
        
        /// <summary>
        /// 所属部门
        /// </summary>
        [Description("所属部门")]
        public string OrgId { get; set; }

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
	    /// 实例备注
	    /// </summary>
         [Description("实例备注")]
        public string Description { get; set; }

    }
}