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
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using Infrastructure.AutoMapper;
using OpenAuth.Repository.Core;
using OpenAuth.Repository.Domain;

namespace OpenAuth.App.Request
{
    /// <summary>
	/// 
	/// </summary>
    [Table("moduleflowscheme")]
    [AutoMapTo(typeof(ModuleFlowScheme))]
    public partial class AddOrUpdateModuleFlowSchemeReq 
    {

        /// <summary>
        /// 
        /// </summary>
        public string Id { get; set; }
        public string ModuleId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string FlowSchemeId { get; set; }
        
         //todo:添加自己的请求字段
    }
}