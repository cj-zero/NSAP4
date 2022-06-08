using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OpenAuth.App.WMS.Request
{
    public class ProductReceiptDetailReq
    {
        public int sbo_id { get; set; }
        public int DocEntry { get; set; }
        public int LineNum { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public int? TargetType { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public int? TrgetEntry { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string BaseRef { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public int? BaseType { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public int? BaseEntry { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public int? BaseLine { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string LineStatus { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string ItemCode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string Dscription { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public decimal? Quantity { get; set; }
        
        /// <summary>
        /// 
        /// </summary>
        
        public decimal? Price { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public string Currency { get; set; }
        /// <summary>
        /// 
        /// </summary>
        
        public decimal? Rate { get; set; }
        
        /// <summary>
        /// 
        /// </summary>
        
        public decimal? LineTotal { get; set; }
        
        /// <summary>
        /// 
        /// </summary>
        
        public string WhsCode { get; set; }
        
    }
}
