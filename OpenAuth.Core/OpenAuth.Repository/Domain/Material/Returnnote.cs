﻿//------------------------------------------------------------------------------
//     This code was generated by a CodeSmith Template.
//
//     DO NOT MODIFY contents of this file. Changes to this
//     file will be lost if the code is regenerated.
//     Author:Yubao Li
//------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using OpenAuth.Repository.Core;

namespace OpenAuth.Repository.Domain
{
    /// <summary>
    ///退料表
    /// </summary>
    [Table("returnnote")]
    public class ReturnNote : BaseEntity<int>
    {
        public ReturnNote()
        {
            this.CreateTime = DateTime.Now; ;
            this.FlowInstanceId = "";
            this.ServiceOrderId = 0;
            this.ServiceOrderSapId = 0;
            this.CreateUser = "";
            this.Status = 0;
            this.CreateUserId = "";

        }

        public int Id { get; set; }
        /// <summary>
        ///创建时间
        /// </summary>
        [Description("创建时间")]
        public DateTime CreateTime { get; set; }

        /// <summary>
        ///工作流程Id
        /// </summary>
        [Description("工作流程Id")]
        public string FlowInstanceId { get; set; }

        /// <summary>
        ///服务单主键Id
        /// </summary>
        [Description("服务单主键Id")]
        public int ServiceOrderId { get; set; }

        /// <summary>
        ///SAP服务Id
        /// </summary>
        [Description("SAP服务Id")]
        public int ServiceOrderSapId { get; set; }

        /// <summary>
        ///创建人名
        /// </summary>
        [Description("创建人名")]
        public string CreateUser { get; set; }

        /// <summary>
        ///退料单状态 1 待退料 2 已退料
        /// </summary>
        [Description("退料单状态 1 待退料 2 已退料")]
        public int? Status { get; set; }

        /// <summary>
        ///创建人Id
        /// </summary>
        [Description("创建人Id")]
        public string CreateUserId { get; set; }

        /// <summary>
        /// 是否最后一次退料
        /// </summary>
        [Description("是否最后一次退料")]
        public int IsLast { get; set; }

        /// <summary>
        /// 签收备注
        /// </summary>
        [Description("签收备注")]
        public string Remark { get; set; }

        /// <summary>
        /// 物流表
        /// </summary>
        public virtual List<Expressage> Expressages { get; set; }


        /// <summary>
        /// 退料单详细列表
        /// </summary>
        public virtual List<ReturnnoteMaterial> ReturnnoteMaterials { get; set; }

        public override void GenerateDefaultKeyVal()
        {

        }

        public override bool KeyIsNull()
        {
            return Id == 0;
        }
    }
}