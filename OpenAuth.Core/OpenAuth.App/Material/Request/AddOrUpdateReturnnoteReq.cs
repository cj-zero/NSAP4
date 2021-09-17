using Infrastructure.AutoMapper;
using OpenAuth.Repository.Domain;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace OpenAuth.App.Material.Request
{
    /// <summary>
    /// 退料单
    /// </summary>
    [Table("Returnnote")]
    [AutoMapTo(typeof(ReturnNote))]
    public class AddOrUpdateReturnnoteReq
    {
        

        /// <summary>
        ///工作流程Id
        /// </summary>
        public string FlowInstanceId { get; set; }

        /// <summary>
        ///服务单主键Id
        /// </summary>
        public int ServiceOrderId { get; set; }

        /// <summary>
        ///退料单id
        /// </summary>
        public int ReturnNoteId { get; set; }


        /// <summary>
        ///SAP服务Id
        /// </summary>
        public int ServiceOrderSapId { get; set; }

        /// <summary>
        ///退料单状态 
        /// </summary>
        public int? Status { get; set; }

        /// <summary>
        ///应收发票id
        /// </summary>
        public int? InvoiceDocEntry { get; set; }

        /// <summary>
        ///销售订单id
        /// </summary>
        public int? SalesOrderId { get; set; }

        /// <summary>
        ///创建人名
        /// </summary>
        public string CreateUser { get; set; }
        /// <summary>
        ///创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }

        /// <summary>
        ///创建人Id
        /// </summary>
        public string CreateUserId { get; set; }


        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }

        /// <summary>
        ///运费
        /// </summary>
        public decimal? FreightCharge { get; set; }

        /// <summary>
        ///快递单号
        /// </summary>
        public string ExpressNumber { get; set; }

        /// <summary>
        ///appid
        /// </summary>
        public int? AppUserId { get; set; }

        /// <summary>
        ///是否存为草稿
        /// </summary>
        public bool IsDraft { get; set; }

        /// <summary>
        /// 退货方式  1自带 2快递
        /// </summary>
        public string DeliveryMethod { get; set; }

        /// <summary>
        /// 总金额
        /// </summary>
        public decimal TotalMoney { get; set; }

        /// <summary>
        /// 退料文件表
        /// </summary>
        public virtual List<ReturnNotePicture> ReturnNotePictures { get; set; }

        /// <summary>
        /// 退料单序列号列表
        /// </summary>
        public virtual List<ReturnNoteProduct> ReturnNoteProducts { get; set; }
    }
}
