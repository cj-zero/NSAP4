using OpenAuth.App.Request;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Material.Request
{
    public class QueryQuotationListReq : PageReq
    {
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
        /// 序列号
        /// </summary>
        public string ManufacturerSerialNumbers { get; set; }
        /// <summary>
        /// 零件名称
        /// </summary>
        public string PartCode { get; set; }

        /// <summary>
        /// 物料类型
        /// </summary>
        public string MaterialType { get; set; }

        /// <summary>
        /// 是否出库
        /// </summary>
        public int? Status { get; set; }

        /// <summary>
        /// 是否查看销售订单
        /// </summary>
        public bool? IsSalesOrderList { get; set; }


    }
}
