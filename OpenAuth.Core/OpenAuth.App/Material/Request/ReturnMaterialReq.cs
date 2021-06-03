using OpenAuth.App.Request;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Material.Request
{
    public class ReturnMaterialReq: PageReq
    {
        /// <summary>
        /// 退料单id
        /// </summary>
        public int? returnNoteId { get; set; }
        /// <summary>
        /// 服务单主键Id
        /// </summary>
        public int ServiceOrderId { get; set; }

        /// <summary>
        /// SAP服务Id
        /// </summary>
        public int? SapId { get; set; }

        /// <summary>
        /// 客户代码or客户名称
        /// </summary>
        public string Customer { get; set; }
        /// <summary>
        /// 当前登陆者用户Id
        /// </summary>
        public int? AppUserId { get; set; }

        /// <summary>
        /// 快递单号
        /// </summary>
        public string ExpressNumber { get; set; }


        /// <summary>
        /// 运费
        /// </summary>
        public decimal FreightCharge { get; set; }

        /// <summary>
        /// 销售订单id
        /// </summary>
        public int? SalesOrderId { get; set; }

        /// <summary>
        /// 应收发票ID
        /// </summary>
        public int? InvoiceDocEntry { get; set; }

        /// <summary>
        /// 页面类型 1.仓库收货 2.品质检验 3.老板审批 4.仓库入库
        /// </summary>
        public int? PageType { get; set; }

        /// <summary>
        /// 页面状态 1.未审批 2.已审批
        /// </summary>
        public int? PageStatus { get; set; }

        /// <summary>
        /// 退料单状态
        /// </summary>
        public int? Status { get; set; }

        /// <summary>
        /// 开始日期
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// 结束日期
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// 是否修改
        /// </summary>
        public bool? IsUpDate { get; set; }

        /// <summary>
        /// 物流图片
        /// </summary>
        public List<string> ExpressPictureIds { get; set; }

        /// <summary>
        /// 退料详情
        /// </summary>
        public List<ReturnMaterialDetail> ReturnMaterialDetail { get; set; }
    }

    public class ReturnMaterialDetail
    {
        /// <summary>
        /// 领料明细Id
        /// </summary>
        public string QuotationMaterialId { get; set; }

        /// <summary>
        /// 物料编码
        /// </summary>
        public string MaterialCode { get; set; }

        /// <summary>
        /// 物料描述
        /// </summary>
        public string MaterialDescription { get; set; }

        /// <summary>
        /// 本次退还数量
        /// </summary>
        public int ReturnQty { get; set; }

        /// <summary>
        /// 需退总计
        /// </summary>
        public int TotalQty { get; set; }

        /// <summary>
        /// 退料图片Id
        /// </summary>
        public string PictureId { get; set; }

        /// <summary>
        /// 成本价
        /// </summary>
        public decimal CostPrice { get; set; }

        /// <summary>
        /// 剩余需退
        /// </summary>
        public int SurplusQty { get; set; }

        /// <summary>
        /// 折后价
        /// </summary>
        public decimal DiscountPrices { get; set; }
    }
}
