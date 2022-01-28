using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Request
{
    public class QueryProductionOrderReq : PageReq
    {
        public QueryProductionOrderReq()
        {
        }
        /// <summary>
        /// 单号
        /// </summary>
        public int? DocEntry { get; set; }

        public List<int?> ProductionId { get; set; }
        /// <summary>
        /// 仓库
        /// </summary>
        public string WareHouse { get; set; }
        /// <summary>
        /// 物料编码
        /// </summary>
        public string ItemCode { get; set; }
        /// <summary>
        /// 生产部门
        /// </summary>
        public string ProductionOrg { get; set; }
        /// <summary>
        /// 部门主管
        /// </summary>
        public string ProductionOrgManager { get; set; }
        /// <summary>
        /// 序列号
        /// </summary>
        public string ManufSN { get; set; }
        /// <summary>
        /// 客户代码
        /// </summary>
        public string CardCode { get; set; }
        /// <summary>
        /// 客户名称
        /// </summary>
        public string CardName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int QueryType { get; set; }
        /// <summary>
        /// 物料信息
        /// </summary>
        public List<MaterialInfo> MaterialInfos { get; set; }
    }

    public class MaterialInfo
    {
        /// <summary>
        /// 物料编码
        /// </summary>
        public string MaterialCode { get; set; }
        /// <summary>
        /// 物料编码
        /// </summary>
        public string MaterialDescription { get; set; }
        /// <summary>
        /// 仓库
        /// </summary>
        public string[] WareHouse { get; set; }
    }
}
