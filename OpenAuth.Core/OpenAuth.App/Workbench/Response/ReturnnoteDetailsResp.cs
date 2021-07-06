﻿using OpenAuth.App.Response;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Workbench.Response
{
    public class ReturnnoteDetailsResp
    {
        /// <summary>
        ///创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }

        /// <summary>
        ///工作流程Id
        /// </summary>
        public string FlowInstanceId { get; set; }

        /// <summary>
        ///服务单主键Id
        /// </summary>
        public int ServiceOrderId { get; set; }

        /// <summary>
        ///SAP服务Id
        /// </summary>
        public int ServiceOrderSapId { get; set; }

        /// <summary>
        ///销售订单
        /// </summary>
        public int SalesOrderId { get; set; }

        /// <summary>
        ///创建人名
        /// </summary>
        public string CreateUser { get; set; }


        /// <summary>
        ///创建人Id
        /// </summary>
        public string CreateUserId { get; set; }

        /// <summary>
        /// 签收备注
        /// </summary>
        public string Remark { get; set; }

        /// <summary>
        /// 修改时间
        /// </summary>
        public DateTime? UpdateTime { get; set; }


        /// <summary>
        /// 总金额
        /// </summary>
        public decimal TotalMoney { get; set; }

        /// <summary>
        /// 是否已清
        /// </summary>
        public bool? IsLiquidated { get; set; }

        /// <summary>
        /// 退货方式  1自带 2快递
        /// </summary>
        public int? DeliveryMethod { get; set; }

        /// <summary>
        ///运费
        /// </summary>
        public decimal? FreightCharge { get; set; }

        /// <summary>
        ///快递单号
        /// </summary>
        public string ExpressNumber { get; set; }

        /// <summary>
        /// 退料单详细列表
        /// </summary>
        public virtual List<ReturnnoteMaterialResp> ReturnnoteMaterials { get; set; }

        /// <summary>
        /// 退料文件表
        /// </summary>
        public virtual List<FileResp> ReturnNotePictures { get; set; }
    }
    /// <summary>
    /// 退料单详细列表
    /// </summary>
    public class ReturnnoteMaterialResp 
    {
        /// <summary>
        ///物料编码
        /// </summary>
        public string MaterialCode { get; set; }

        /// <summary>
        ///本次退还数量
        /// </summary>
        public int? Count { get; set; }

        /// <summary>
        ///收货备注
        /// </summary>
        public string ReceivingRemark { get; set; }

        /// <summary>
        ///发货备注
        /// </summary>
        public string ShippingRemark { get; set; }

        /// <summary>
        ///退料单Id
        /// </summary>
        public int? ReturnNoteId { get; set; }


        /// <summary>
        ///物料描述
        /// </summary>
        public string MaterialDescription { get; set; }


        /// <summary>
        ///良品数量
        /// </summary>
        public int? GoodQty { get; set; }

        /// <summary>
        ///次品数量
        /// </summary>
        public int? SecondQty { get; set; }

        /// <summary>
        ///应收发票id
        /// </summary>
        public int? InvoiceDocEntry { get; set; }

        /// <summary>
        ///良品仓库
        /// </summary>
        public string GoodWhsCode { get; set; }

        /// <summary>
        ///次品仓库
        /// </summary>
        public string SecondWhsCode { get; set; }

        /// <summary>
        ///产品编号
        /// </summary>
        public string ProductCode { get; set; }

        /// <summary>
        ///原产品编码
        /// </summary>
        public string ReplaceProductCode { get; set; }

        /// <summary>
        ///退料图片
        /// </summary>
        public List<FileResp> Files { get; set; }

    }

}
