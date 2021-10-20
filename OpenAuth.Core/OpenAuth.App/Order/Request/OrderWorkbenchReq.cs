using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Order.Request
{
    /// <summary>
    /// 提交给我的
    /// </summary>
    public class OrderSubmtToMeReq
    {
        /// <summary>
        /// 
        /// </summary>
        public int page { get; set; } = 1;
        /// <summary>
        /// 
        /// </summary>
        public int rp { get; set; } = 20;
        /// <summary>
        /// 
        /// </summary>
        public string qtype { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string query { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string sortname { get; set; } = "upd_dt";
        /// <summary>
        /// 
        /// </summary>
        public string sortorder { get; set; } = "desc";
        /// <summary>
        /// 
        /// </summary>
      //  public string types { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Applicator { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Customer { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Status { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string BeginDate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string EndDate { get; set; }
    }
}
