using OpenAuth.Repository.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace OpenAuth.Repository.Domain
{
    /// <summary>
    /// 近五年有交货记录的客户地址信息
    /// </summary>
    [Table("customerlocation")]
    public class CustomerLocation : Entity
    {
        public string CardCode { get; set; }
        public string CardName { get; set; }
        /// <summary>
        /// 经度
        /// </summary>
        [Description("经度")]
        public decimal? Longitude { get; set; }
        /// <summary>
        /// 纬度
        /// </summary>
        [Description("纬度")]
        public decimal? Latitude { get; set; }
        /// <summary>
        /// 国家
        /// </summary>
        [Description("国家")]
        public string Country { get; set; }
        /// <summary>
        /// 省
        /// </summary>
        [Description("省")]
        public string Province { get; set; }
        /// <summary>
        /// 市
        /// </summary>
        [Description("市")]
        public string City { get; set; }
        /// <summary>
        /// 区
        /// </summary>
        [Description("区")]
        public string Area { get; set; }

        /// <summary>
        /// 详细地址
        /// </summary>
        [Description("详细地址")]
        public string Addr { get; set; }

        /// <summary>
        /// 定位时间
        /// </summary>
        [Description("定位时间")]
        public DateTime CreateTime { get; set; }
    }
}
