using OpenAuth.App.Request;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Nwcali.Request
{
    public class QueryPortfolioAssetListReq : PageReq
    {
        /// <summary>
        /// 资产Id
        /// </summary>
        public string AssetsId { get; set; }

        /// <summary>
        /// 组合资产名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 组合资产类别
        /// </summary>
        public int? Category { get; set; }

        /// <summary>
        /// 固定配件数
        /// </summary>
        public int? FirmwareParts { get; set; }

        /// <summary>
        /// 临时配件数
        /// </summary>
        public int? TemporaryParts { get; set; }

        /// <summary>
        /// 创建人
        /// </summary>
        public string CreateUser { get; set; }

        /// <summary>
        /// 部门
        /// </summary>
        public string OrgName { get; set; }


        /// <summary>
        /// 创建时间 
        /// </summary>
        public DateTime? CreateStartTime { get; set; }
        public DateTime? CreateEndTime { get; set; }

        /// <summary>
        /// 修改时间 
        /// </summary>
        public DateTime? UpdateStartTime { get; set; }
        public DateTime? UpdateEndTime { get; set; }
    }
}
