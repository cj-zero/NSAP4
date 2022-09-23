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
    public class LimsInfo : BaseEntity<int>
    {
        /// <summary>
        /// 客户编码
        /// </summary>
        public string CardCode { get; set; }
        /// <summary>
        /// 用户id
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// 类型
        /// </summary>
        public string Type { get; set; }

        public override void GenerateDefaultKeyVal()
        {
        }

        public override bool KeyIsNull()
        {
            return Id == 0;
        }
    }
}
