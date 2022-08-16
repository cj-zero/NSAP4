/*
 * @author : wangying
 * @date : 2022-8-5
 * @desc : 常用语
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
    /// 常用语
    /// </summary>
    [Table("client_followup_phrase")]
    public class ClientFollowUpPhrase : BaseEntity<int>
    {
        /// <summary>
        /// 业务员编码
        /// </summary>
        public int SlpCode { get; set; }

        /// <summary>
        /// 内容
        /// </summary>
        public string Remark { get; set; }

        /// <summary>
        /// 类型（0 跟进 1 日程）
        /// </summary>
        public int Type { get; set; }
        
        /// <summary>
        /// 创建人
        /// </summary>
        public string CreateUser { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime? CreateDate { get; set; }
      
        public override void GenerateDefaultKeyVal()
        {
        }

        public override bool KeyIsNull()
        {
            return Id == 0;
        }
    }
}
