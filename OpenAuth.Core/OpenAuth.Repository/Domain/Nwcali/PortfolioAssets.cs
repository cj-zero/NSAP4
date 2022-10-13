using OpenAuth.Repository.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace OpenAuth.Repository.Domain.Nwcali
{
    /// <summary>
    /// 组合资产表
    /// </summary>
    [Table("PortfolioAssets")]
    public partial class PortfolioAssets :  BaseEntity<int>
    {
        public string Guid { get; set; }

        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 类别
        /// </summary>
        public int Category { get; set; }

        /// <summary>
        /// 固件配件数
        /// </summary>
        public int FirmwareParts { get; set; }

        /// <summary>
        /// 临时配件数
        /// </summary>
        public int TemporaryParts { get; set; }

        /// <summary>
        /// 部门
        /// </summary>
        public string OrgName { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }

        /// <summary>
        /// 创建时间 
        /// </summary>
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 修改时间 
        /// </summary>
        public DateTime UpdateTime { get; set; }

        /// <summary>
        /// 创建人
        /// </summary>
        public string CreateUser { get; set; }

        /// <summary>
        /// 创建人ID
        /// </summary>
        public string CreateUserId { get; set; }

        public override void GenerateDefaultKeyVal()
        {

        }
        public override bool KeyIsNull()
        {
            return Id == 0;
        }
    }

}
