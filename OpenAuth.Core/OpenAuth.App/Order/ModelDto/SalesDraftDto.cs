using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Order
{
    /// <summary>
    /// 销售报价单
    /// </summary>
    public class SalesDraftDto
    {
        /// <summary>
        /// 行号
        /// </summary>
        public int RowNumber { get; set; }
        /// <summary>
        /// 更新日期
        /// </summary>
        public DateTime UpdateDate { get; set; }
        /// <summary>
        /// 单据编号
        /// </summary>
        public int DocEntry { get; set; }
        /// <summary>
        /// 客户代码
        /// </summary>
        public string CardCode { get; set; }
        /// <summary>
        /// 客户名称
        /// </summary>
        public string CardName { get; set; }
        /// <summary>
        /// 单据总金额
        /// </summary>
        public decimal DocTotal { get; set; }
        /// <summary>
        /// 未清金额
        /// </summary>
        public decimal OpenDocTotal { get; set; }
        /// <summary>
        /// 创建日期
        /// </summary>
        public DateTime CreateDate { get; set; }
        /// <summary>
        /// 销售员
        /// </summary>
        public string SlpName { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string Comments { get; set; }
        /// <summary>
        /// 状态
        /// </summary>
        public string DocStatus { get; set; }
    }
    public class SboInfoDto
    {
        /// <summary>
        /// Id
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }
    }
}
