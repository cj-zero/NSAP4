using OpenAuth.App.Response;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Workbench.Response
{
    public class ReturnnoteDetailsResp
    {
        
        /// <summary>
        ///退料单id
        /// </summary>
        public DateTime ReturnnoteId { get; set; }

        /// <summary>
        /// 签收备注
        /// </summary>
        public string Remark { get; set; }

        /// <summary>
        /// 修改时间
        /// </summary>
        public string UpdateTime { get; set; }


        /// <summary>
        /// 总金额
        /// </summary>
        public decimal TotalMoney { get; set; }

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
        /// 流程
        /// </summary>
        public List<FlowPathResp> FlowPathResp { get; set; }

        /// <summary>
        /// 退料单详细列表
        /// </summary>
        public virtual List<ReturnNoteMaterialResp> ReturnNoteMaterials { get; set; }

        /// <summary>
        /// 退料文件表
        /// </summary>
        public virtual List<FileResp> ReturnNotePictures { get; set; }
        /// <summary>
        /// 退料单操作历史
        /// </summary>
        public virtual List<OperationHistoryResp> ReturnNoteHistoryResp { get; set; }
    }
    /// <summary>
    /// 退料单详细列表
    /// </summary>
    public class ReturnNoteMaterialResp 
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

        /// <summary>
        ///退料图片
        /// </summary>
        public List<ReturnnoteMaterialNumberResp> ReturnnoteMaterialNumberResps { get; set; }


    }
    public class ReturnnoteMaterialNumberResp 
    {
        /// <summary>
        ///退料单物料d
        /// </summary>
        public string ReturnnoteMaterialId { get; set; }

        /// <summary>
        ///退仓的序列号
        /// </summary>
        public string ReturnNumber { get; set; }

        /// <summary>
        ///领取的序列号
        /// </summary>
        public string RemoveNumber { get; set; }
    }
}
