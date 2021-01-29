using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Material.Request
{
    public class ReturnMaterialReq
    {
        /// <summary>
        /// 服务单主键Id
        /// </summary>
        public int ServiceOrderId { get; set; }

        /// <summary>
        /// SAP服务Id
        /// </summary>
        public int SapId { get; set; }

        /// <summary>
        /// 出库单Id集合
        /// </summary>
        public List<int> StockOutIds { get; set; }

        /// <summary>
        /// 当前登陆者用户Id
        /// </summary>
        public int AppUserId { get; set; }

        /// <summary>
        /// 快递单号
        /// </summary>
        public string TrackNumber { get; set; }

        /// <summary>
        /// 是否最后一次退料
        /// </summary>
        public int IsLastReturn { get; set; }

        /// <summary>
        /// 退料详情
        /// </summary>
        public List<ReturnMaterialDetail> ReturnMaterialDetail { get; set; }
    }

    public class ReturnMaterialDetail
    {
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
    }
}
