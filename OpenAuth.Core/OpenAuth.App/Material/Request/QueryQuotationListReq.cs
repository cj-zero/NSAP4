using OpenAuth.App.Request;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Material.Request
{
    public class QueryQuotationListReq : PageReq
    {
        public QueryQuotationListReq() 
        {
            WhsCode = "37";
        }
        /// <summary>
        /// appid
        /// </summary>
        public int? AppId { get; set; }
        /// <summary>
        /// 状态栏
        /// </summary>
        public int? StartType { get; set; }

        /// <summary>
        /// 报价单号
        /// </summary>
        public int? QuotationId { get; set; }

        /// <summary>
        /// 打印报价单号
        /// </summary>
        public string serialNumber { get; set; }
        
        /// <summary>
        /// 销售单号
        /// </summary>
        public int? SalesOrderId { get; set; }

        /// <summary>
        ///报价单审批状态
        /// </summary>
        public decimal? QuotationStatus { get; set; }

        /// <summary>
        /// 客户
        /// </summary>
        public string CardCode { get; set; }

        /// <summary>
        /// 服务单SapId
        /// </summary>
        public int? ServiceOrderSapId { get; set; }

        /// <summary>
        /// 服务单Id
        /// </summary>
        public int? ServiceOrderId { get; set; }

        /// <summary>
        /// 申请人
        /// </summary>
        public string CreateUser { get; set; }

        /// <summary>
        /// 申请人ID
        /// </summary>
        public string CreateUserId { get; set; }

        /// <summary>
        /// 创建日期开始
        /// </summary>
        public DateTime? StartCreateTime { get; set; }

        /// <summary>
        /// 创建日期结束
        /// </summary>
        public DateTime? EndCreateTime { get; set; }
        /// <summary>
        /// 设备名称
        /// </summary>
        public string MaterialCode { get; set; }

        /// <summary>
        /// 退料传递设备类型
        /// </summary>
        public string ItemCode { get; set; }
        /// <summary>
        /// 序列号
        /// </summary>
        public string ManufacturerSerialNumbers { get; set; }
        /// <summary>
        /// 零件名称
        /// </summary>
        public string PartCode { get; set; }

        /// <summary>
        /// 被替换零件描述
        /// </summary>
        public string PartDescribe { get; set; }

        /// <summary>
        /// 被替换零件编码
        /// </summary>
        public string ReplacePartCode { get; set; }

        /// <summary>
        /// 物料类型
        /// </summary>
        public string MaterialType { get; set; }

        /// <summary>
        /// 是否出库
        /// </summary>
        public int? Status { get; set; }

        /// <summary>
        /// 页面状态 1报价单审批 2 销售单审批 3出库
        /// </summary>
        public int? PageStart { get; set; }

        /// <summary>
        /// 是否修改
        /// </summary>
        public bool? IsUpdate { get; set; }
        /// <summary>
        /// 是否延保
        /// </summary>
        public bool? IsWarranty { get; set; }
        
        /// <summary>
        /// 仓库号
        /// </summary>
        public string WhsCode { get; set; }
        /// <summary>
        /// App查看物料及描述
        /// </summary>
        public string AppPartCode { get; set; }
        /// <summary>
        /// 取消筛选条件
        /// </summary>
        public string CancelRequest { get; set; }
        /// <summary>
        /// 交货
        /// </summary>
        public string SalesOfDeliveryId { get; set; }
        /// <summary>
        /// 是否驳回
        /// </summary>
        public bool? IsReject { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }

        /// <summary>
        /// 是否常用物料
        /// </summary>
        public bool? IsCommonUsed { get; set; }
        /// <summary>
        /// 首页报表查询条件
        /// </summary>
        public int? IsFinlish { get; set; }
        public int? PageType { get; set; }
        /// <summary>
        /// 查询类型 1.领料
        /// </summary>
        public int? QueryType { get; set; }

        /// <summary>
        /// 查询类型 (判断是否为ECN)
        /// </summary>
        public int? FromId { get; set; }
        

        /// <summary>
        /// 打印物料id
        /// </summary>
        public List<QuotationMergeMaterialReq> QuotationMergeMaterialReqs { get; set; }
    }

    public class SyncSalesOrder
    {
        public string QuotationId { get; set; }
    }
}
