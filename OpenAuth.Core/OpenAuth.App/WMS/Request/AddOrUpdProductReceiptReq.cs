using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Infrastructure.AutoMapper;
using System.ComponentModel.DataAnnotations.Schema;
using OpenAuth.Repository.Domain;

namespace OpenAuth.App.WMS.Request
{
    [Table("product_oign")]
    [AutoMapTo(typeof(product_oign))]
    public class AddOrUpdProductReceiptReq
    {
        public int sbo_id { get; set; }
        public int DocEntry { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public int DocNum { get; set; }
       
        
        public System.DateTime? DocDate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public System.DateTime? DocDueDate { get; set; }
        
        /// <summary>
        /// 
        /// </summary>
        
        public string Comments { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string JrnlMemo { get; set; }
        
        /// <summary>
        /// 
        /// </summary>
        
        public string U_CPH { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string U_YSQX { get; set; }
        
        /// <summary>
        /// 
        /// </summary>
        
        public string U_YGMD { get; set; }
        
        /// <summary>
        /// 
        /// </summary>
        
        public string U_PRX_TkNo { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string U_PRX_SRVR { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string U_ShipName { get; set; }
        
        /// <summary>
        /// 
        /// </summary>
        
        public string U_SMAZ { get; set; }
        
     
        public virtual IList<ProductReceiptDetailReq> ProductReceiptDetailReqs { get; set; }
    }
}
