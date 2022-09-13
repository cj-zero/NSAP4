using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Request
{
    public class MaterialRecordReq
    {

        /// <summary>
        /// 物料代码
        /// </summary>
        public string MaterialCode { get; set; }

        /// <summary>
        /// 申请人
        /// </summary>
        public string CreateUser { get; set; }

        /// <summary>
        /// 单据编号
        /// </summary>
        public string DocEntry { get; set; }

        /// <summary>
        /// 客户代码
        /// </summary>
        public string CustomerId { get; set; }

        /// <summary>
        /// 仓库
        /// </summary>
        public string WhsCode { get; set; }

        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime? StartTime { get; set; }
        /// <summary>
        /// 结束时间
        /// </summary>
        public DateTime? EndTime { get; set; }
        public int pageIndex { get; set; }
        public int pageSize { get; set; }
    }
}
