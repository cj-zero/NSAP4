using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using OpenAuth.Repository.Core;

namespace OpenAuth.Repository.Domain
{
    /// <summary>
	/// 
	/// </summary>
    [Table("materialssettlement")]
    public partial class MaterialsSettlement : BaseEntity<int>
    {
        public MaterialsSettlement()
        {
        }

        /// <summary>
        /// 服务单id
        /// </summary>
        [Description("服务单id")]
        public int? ServerOrderId { get; set; }

        /// <summary>
        /// 出库单id
        /// </summary>
        [Description("出库单id")]
        public int? QuotationID { get; set; }
        
        /// <summary>
        /// 申请人姓名
        /// </summary>
        [Description("申请人姓名")]
        public string ProposerName { get; set; }

        /// <summary>
        /// 申请人id
        /// </summary>
        [Description("申请人id")]
        public string ProposerId { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        [Description("备注")]
        public string Remark { get; set; }
        /// <summary>
        /// 总金额
        /// </summary>
        [Description("总金额")]
        public decimal? TotalMoney { get; set; }

        /// <summary>
        /// 已结算金额
        /// </summary>
        [Description("已结算金额")]
        public decimal? Totalpayamount { get; set; }

        /// <summary>
        /// 状态
        /// </summary>
        [Description("状态")]
        public int? State { get; set; }
        
        /// <summary>
        /// 创建人id
        /// </summary>
        [Description("创建人id")]
        public string CreateUserId { get; set; }
        /// <summary>
        /// 创建人姓名
        /// </summary>
        [Description("创建人姓名")]
        public string CreateUser { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        [Description("创建时间")]
        public System.DateTime CreateTime { get; set; }


        public override void GenerateDefaultKeyVal()
        {

        }

        public override bool KeyIsNull()
        {
            return Id == 0;
        }
    }
}