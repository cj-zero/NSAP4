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
        public string ReturnnoteId { get; set; }

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
        /// 退料文件表
        /// </summary>
        public virtual List<ReturnNoteProductResp> ReturnNoteProducts { get; set; }
        
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
        public string Id { get; set; }
        /// <summary>
        /// 物料类型
        /// </summary>
        public int? MaterialType { get; set; }
        /// <summary>
        ///物料编码
        /// </summary>
        public string MaterialCode { get; set; }

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
        public bool? IsGood { get; set; }


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
        ///原物料编码
        /// </summary>
        public string ReplaceMaterialCode { get; set; }

        /// <summary>
        ///原物料描述
        /// </summary>
        public string ReplaceMaterialDescription { get; set; }

        /// <summary>
        ///SN号和PN号
        /// </summary>
        public string SNandPN { get; set; }
        /// <summary>
        ///原物料SN号和PN号
        /// </summary>
        public string ReplaceSNandPN { get; set; }

        /// <summary>
        ///序列号表id
        /// </summary>
        public string ReturnNoteProductId { get; set; }

        /// <summary>
        ///金额
        /// </summary>
        public decimal Money { get; set; }
        /// <summary>
        ///报价单物料id
        /// </summary>
        public string QuotationMaterialId { get; set; }

        /// <summary>
        ///退料图片
        /// </summary>
        public List<FileResp> Files { get; set; }

    }
    public class ReturnNoteProductResp
    {
        /// <summary>
        ///退料单Id
        /// </summary>
        public int ReturnNoteId { get; set; }

        /// <summary>
        ///产品编码
        /// </summary>
        public string ProductCode { get; set; }

        /// <summary>
        ///物料编码
        /// </summary>
        public string MaterialCode { get; set; }

        /// <summary>
        ///物料描述
        /// </summary>
        public string MaterialDescription { get; set; }
        /// <summary>
        ///小计金额
        /// </summary>
        public decimal Money { get; set; }

        /// <summary>
        /// 物料列表
        /// </summary>
        public virtual List<ReturnNoteMaterialResp> ReturnNoteMaterials { get; set; }
    }
}
