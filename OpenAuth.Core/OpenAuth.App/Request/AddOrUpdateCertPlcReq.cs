﻿//------------------------------------------------------------------------------
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
using OpenAuth.Repository.Core;

namespace OpenAuth.App.Request
{
    /// <summary>
	/// 
	/// </summary>
    [Table("certplc")]
    public partial class AddOrUpdateCertPlcReq 
    {

        /// <summary>
        /// 
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string CertNo { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string PlcGuid { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public System.DateTime CreateTime { get; set; }
        
         //todo:添加自己的请求字段
    }
}