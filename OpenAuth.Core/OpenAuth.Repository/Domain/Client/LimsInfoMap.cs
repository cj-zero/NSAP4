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
    [Table("client_limsinfomap")]
    public class LimsInfoMap : BaseEntity<int>
    {

        /// <summary>
        /// limsInfo主键
        /// </summary>
        public int LimsInfoId { get; set; }
        /// <summary>
        /// 客户编码
        /// </summary>
        public string CardCode { get; set; }
        /// <summary>
        /// 客户编码
        /// </summary>
        public string CardName { get; set; }
        /// <summary>
        /// 创建人
        /// </summary>
        public string CreateUser { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateDate { get; set; }

        public override void GenerateDefaultKeyVal()
        {
        }

        public override bool KeyIsNull()
        {
            return Id == 0;
        }
    }
}
