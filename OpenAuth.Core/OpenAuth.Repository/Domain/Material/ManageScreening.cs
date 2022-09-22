/*
 * @author : wangying
 * @date : 2022-8-9
 * @desc : 工程部物料设计
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
    /// 工程部物料设计
    /// </summary>
    [Table("manage_screening")]
    public class ManageScreening : BaseEntity<int>
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
        /// 物料名称
        /// </summary>
        public string ItemDesc { get; set; }
        /// <summary>
        /// 业务员编码
        /// </summary>
        public int SlpCode { get; set; }

        /// <summary>
        /// 业务员名称
        /// </summary>
        public string SlpName { get; set; }
        /// <summary>
        /// 提交时间
        /// </summary>
        public DateTime? SubmitTime { get; set; }

        /// <summary>
        /// 合约评审单号
        /// </summary>
        public int? ContractReviewCode { get; set; }
        /// <summary>
        /// 特殊要求
        /// </summary>
        public string custom_req { get; set; }
        /// <summary>
        /// 编码类别
        /// </summary>
        public string ItemTypeName { get; set; }
        /// <summary>
        /// 图纸编码
        /// </summary>
        public string ItemName { get; set; }
        /// <summary>
        /// 配置类型
        /// </summary>
        public string U_ZS { get; set; }
        /// <summary>
        /// 数量
        /// </summary>
        public decimal Quantity { get; set; }

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
        public DateTime? UrlUpdate { get; set; }
        /// <summary>
        /// 样机或批量
        /// </summary>
        public string IsDemo { get; set; }
        /// <summary>
        /// 编辑样机批量时间
        /// </summary>
        public DateTime? DemoUpdate { get; set; }
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
