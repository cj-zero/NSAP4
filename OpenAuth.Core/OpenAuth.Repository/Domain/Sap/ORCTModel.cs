using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.Repository.Domain
{
    /// <summary>
    /// 查询用 非数据库表格
    /// </summary>
    public class ORCTModel
    {
        public ORCTModel()
        {
            
        }
        /// <summary>
        /// 销售单号
        /// </summary>
        public int? OrderNo { get; set; }
        /// <summary>
        /// 已收款金额
        /// </summary>
        public decimal? DocTotal { get; set; }
        /// <summary>
        /// 单据状态
        /// </summary>
        public string DocStatus { get; set; }
        /// <summary>
        /// 单据总金额
        /// </summary>
        public decimal? SEDocTotal { get; set; }
        public int? DocEntry { get; set; }
    }
}
