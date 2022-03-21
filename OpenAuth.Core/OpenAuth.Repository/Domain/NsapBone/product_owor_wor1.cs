using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace OpenAuth.Repository.Domain
{
    /// <summary>
    /// 生产订单关联查询，非数据库表
    /// </summary>
    public class product_owor_wor1
    {
        public product_owor_wor1() {

        }

        public int DocEntry { get; set; }
        public string Code { get; set; }
        public int ProductionId { get; set; }
        public string ItemName { get; set; }
        public string ItemCode { get; set; }
        public decimal? PlannedQty { get; set; }
        public decimal? OpenQty { get; set; }
        public string Org { get; set; }
        public string Warehouse { get; set; }
        public string Remark { get; set; }
        public string FromTheme { get; set; }
        /// <summary>
        /// 完成数量
        /// </summary>
        public decimal? CmpltQty { get; set; }
        public string Version { get; set; }
    }
}
