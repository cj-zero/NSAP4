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
        public int? ChildEntry { get; set; }
        public string ItemCode { get; set; }
        public string PartItemCode { get; set; }
        public decimal? PlannedQty { get; set; }
        public decimal? PartPlannedQty { get; set; }
        /// <summary>
        /// 完成数量
        /// </summary>
        public decimal? CmpltQty { get; set; }
        public string U_WO_LTDW { get; set; }
        public string wareHouse { get; set; }
    }
}
