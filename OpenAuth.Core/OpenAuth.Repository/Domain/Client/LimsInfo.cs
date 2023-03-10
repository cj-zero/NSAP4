/*
 * @author : Eaven
 * @date : 2022-4-20
 * @desc : 客户规则
 */
using OpenAuth.Repository.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace OpenAuth.Repository.Domain
{
    /// <summary>
    /// 客户与LIMS推广员绑定表
    /// </summary>
    [Table("client_limsinfo")]
    public partial class LimsInfo : BaseEntity<int>
    {
        public LimsInfo()
        {
            this.UserId = string.Empty;
            this.Type = string.Empty;
            this.Count = 0;
            this.CreateUser = string.Empty;
            this.CreateDate = DateTime.Now;
        }
        /// <summary>
        /// 用户id
        /// </summary>
        public string UserId { get; set; }
        /// <summary>
        /// 业务员编码
        /// </summary>
        public int SlpCode { get; set; }
        /// <summary>
        /// 类型
        /// </summary>
        public string Type { get; set; }
        /// <summary>
        /// 可推广客户数
        /// </summary>
        public int Count { get; set; }
        /// <summary>
        /// 创建人
        /// </summary>
        public string CreateUser { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateDate { get; set; }
        /// <summary>
        /// 推广员绑定客户集合
        /// </summary>
        public virtual List<LimsInfoMap> LimsInfoMapList { get; set; }

        public override void GenerateDefaultKeyVal()
        {
        }

        public override bool KeyIsNull()
        {
            return Id == 0;
        }
    }
}
