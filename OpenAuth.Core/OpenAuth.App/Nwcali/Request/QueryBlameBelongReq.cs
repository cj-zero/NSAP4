using OpenAuth.App.Request;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Nwcali.Request
{
    public class QueryBlameBelongReq : PageReq
    {
        public int? Id { get; set; }
        /// <summary>
        /// 责任依据 1-服务质量 2-生产质量 3-研发质量 4-采购质量 5-工程设计质量 6-需求不请 7-需求变更
        /// </summary>
        public List<string> QryBasis { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string CardCode { get; set; }

        /// <summary>
        /// 开始日期
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// 结束日期
        /// </summary>
        public DateTime? EndDate { get; set; }
        /// <summary>
        /// 责任单据类型 1-服务单 2-生产单 3-采购单 4-其他
        /// </summary>
        public int? QryDocType { get; set; }
        /// <summary>
        /// 销售单号
        /// </summary>
        public int? SaleOrderId { get; set; }
        /// <summary>
        /// 状态
        /// </summary>
        public int? Status { get; set; }
        /// <summary>
        /// 1-我的提交 2 按灯单据 3-待处理 4-已处理
        /// </summary>
        public int? PageType { get; set; }
        public int? OrderNo { get; set; }

        public string VestinOrg { get; set; }
        /// <summary>
        /// 发起人
        /// </summary>
        public string CreateUser { get; set; }

        /// <summary>
        /// 处理人
        /// </summary>
        public string HandleUser { get; set; }
    }
}
