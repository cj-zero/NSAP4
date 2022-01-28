using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Response
{
    public class SerialNumberResp
    {
        /// <summary>
        /// 
        /// </summary>
        public int InsID { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Customer { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string CustmrName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string ManufSN { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string InternalSN { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string ItemCode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string ItemName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public DateTime? ManufDate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int? DeliveryNo { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public DateTime? DlvryDate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string ServiceFee { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int? ContractId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string SlpName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public DateTime? CntrctStrt { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public DateTime? CntrctEnd { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public DateTime? CreateDate { get; set; }
       
    }

    public class SerialInfo
    {
        /// <summary>
        /// 序列号
        /// </summary>
        public string SerialNum { get; set; }

        /// <summary>
        /// 物料编码
        /// </summary>
        public string MaterialCode { get; set; }

        /// <summary>
        /// 物料描述
        /// </summary>
        public string MaterialDesc { get; set; }

        /// <summary>
        /// 购买金额
        /// </summary>
        public decimal PurchaseAmount { get; set; }

        /// <summary>
        /// 送达时间
        /// </summary>
        public DateTime PurchaseTime { get; set; }

        /// <summary>
        /// 延保到期时间
        /// </summary>
        public DateTime WarrantyTime { get; set; }
    }
}
