﻿//------------------------------------------------------------------------------
// <autogenerated>
//     This code was generated by a CodeSmith Template.
//
//     DO NOT MODIFY contents of this file. Changes to this
//     file will be lost if the code is regenerated.
//     Author:RenChun Xia
// </autogenerated>
//------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using OpenAuth.Repository.Core;

namespace OpenAuth.Repository.Domain
{
    /// <summary>
	/// 流程类型
	/// </summary>
    [Table("wfa_type")]
    public partial class wfa_type
    {
        public wfa_type()
        {
            this.job_type_nm = string.Empty;
            this.user_id = 0;
            this.job_type_desc = string.Empty;
            this.module_id = 0;
            this.valid = 0;
            this.has_flow = 0;
            this.para_cnt = 0;
            this.sync_sap = 0;
            this.show_idx = 0;
            this.after_sql = string.Empty;
            this.upd_dt = DateTime.Now;
        }

        /// <summary>
        /// 流程类型Id
        /// </summary>
        public int? job_type_id { get; set; }

        /// <summary>
        /// 流程类型名称
        /// </summary>
        public string job_type_nm { get; set; }

        /// <summary>
        /// 备注信息
        /// </summary>
        public string job_type_desc { get; set; }

        /// <summary>
        /// 功能模块ID
        /// </summary>
        public int module_id { get; set; }

        /// <summary>
        /// 创建者
        /// </summary>
        public int user_id { get; set; }

        /// <summary>
        /// 是否有效(0否，1是)
        /// </summary>
        public int valid { get; set; }

        /// <summary>
        /// 是否走审核流程(0否，1是)
        /// </summary>
        public int has_flow { get; set; }

        /// <summary>
        /// 自定义参数个数
        /// </summary>
        public int para_cnt { get; set; }

        /// <summary>
        /// 是否同步到SAP B1(0否，1是)
        /// </summary>
        public int sync_sap { get; set; }

        /// <summary>
        /// 同步完成后执行SQL过程(用于同步SAP结果数据)
        /// </summary>
        public string after_sql { get; set; }

        /// <summary>
        /// 显示序号，从1开始
        /// </summary>
        public int show_idx { get; set; }

        /// <summary>
        /// 最后更新时间
        /// </summary>
        public System.DateTime? upd_dt { get; set; }
    }
}