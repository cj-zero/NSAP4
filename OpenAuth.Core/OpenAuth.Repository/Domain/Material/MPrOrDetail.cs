using OpenAuth.Repository.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace OpenAuth.Repository.Domain.Material
{
    [Table("manage_prordetail")]
    public class MPrOrDetail : BaseEntity<int>
    {
        /// <summary>
        /// 单据编码
        /// </summary>
        public string DocEntry { get; set; }
        /// <summary>
        /// 客户编码
        /// </summary>
        public string CardCode { get; set; }
        /// <summary>
        /// 客户名称
        /// </summary>
        public string CardName { get; set; }

        /// <summary>
        /// 物料编码
        /// </summary>
        public string ItemCode { get; set; }



        /// <summary>
        /// 业务员名称
        /// </summary>
        public string SlpName { get; set; }

        /// <summary>
        /// 图纸编码
        /// </summary>
        public string ItemName { get; set; }

        /// <summary>
        /// 生产订单号
        /// </summary>
        public string ProduceNo { get; set; }

        public string IsDemo { get; set; }
        /// <summary>
        /// 数量
        /// </summary>
        public decimal? Quantity { get; set; }

        /// <summary>
        /// 版本号
        /// </summary>
        public string VersionNo { get; set; }
        /// <summary>
        /// 文件路径
        /// </summary>
        public string FileUrl { get; set; }

        /// <summary>
        /// 编辑url时间
        /// </summary>
        public DateTime UrlUpdate { get; set; }


        /// <summary>
        /// 创建人
        /// </summary>
        public string Creator { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateDate { get; set; }

        public int IsDelete { get; set; }

        public override void GenerateDefaultKeyVal()
        {
        }

        public override bool KeyIsNull()
        {
            return Id == 0;
        }


    }
}
