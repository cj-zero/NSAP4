﻿using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using OpenAuth.Repository.Core;

namespace OpenAuth.Repository.Domain
{
    /// <summary>
    /// 烤机工步版本维护
    /// </summary>
    [Table("stepversion")]
    public partial class StepVersion : BaseEntity<int>
    {
        public StepVersion()
        {
            this.StepName = string.Empty;
            this.SeriesName = string.Empty;
            this.StepVersionName = string.Empty;
            this.FilePath = string.Empty;
            this.FileName = string.Empty;
            this.Remark = string.Empty;
        }

        /// <summary>
        /// 工步文件名称
        /// </summary>
        public string StepName { get; set; }

        /// <summary>
        /// 系列名称(4系/5系)
        /// </summary>
        public string SeriesName { get; set; }

        /// <summary>
        /// 工步型号名称（5V6A/5V20A）
        /// </summary> 
        public string StepVersionName { get; set; }

        /// <summary>
        /// 文件路径
        /// </summary>
        public string FilePath { get; set; }

        /// <summary>
        /// 文件名称
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }

        /// <summary>
        /// 创建人Id
        /// </summary>
        public string CreateUserId { get; set; }

        /// <summary>
        /// 创建人
        /// </summary>
        public string CreateUser { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 修改时间
        /// </summary>
        public DateTime UpdateTime { get; set; }

        /// <summary>
        /// 排序(降序)
        /// </summary>
        public int Sorts { get; set; }
        /// <summary>
        /// 是否启用
        /// </summary>
        public bool Status { get; set; }
        public override void GenerateDefaultKeyVal()
        {
        }

        public override bool KeyIsNull()
        {
            return Id == 0;
        }
    }
}
