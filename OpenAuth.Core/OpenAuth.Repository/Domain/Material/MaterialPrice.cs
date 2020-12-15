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
    [Table("materialprice")]
    public partial class MaterialPrice : Entity
    {
        public MaterialPrice()
        {
          this.MaterialCode= string.Empty;
          this.CreateId= string.Empty;
          this.CreateName= string.Empty;
          this.CreateTime= DateTime.Now;
        }

        
        /// <summary>
        /// 物料编号
        /// </summary>
        [Description("物料编号")]
        public string MaterialCode { get; set; }
        /// <summary>
        /// 固定价格模式
        /// </summary>
        [Description("固定价格模式")]
        public decimal? SettlementPriceModel { get; set; }
        /// <summary>
        /// 固定价格
        /// </summary>
        [Description("固定价格")]
        public decimal? SettlementPrice { get; set; }
        /// <summary>
        /// 创建人id
        /// </summary>
        [Description("创建人id")]
        public string CreateId { get; set; }
        /// <summary>
        /// 创建人姓名
        /// </summary>
        [Description("创建人姓名")]
        public string CreateName { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        [Description("创建时间")]
        public System.DateTime CreateTime { get; set; }
    }
}