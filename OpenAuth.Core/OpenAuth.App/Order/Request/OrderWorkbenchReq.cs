using OpenAuth.App.Request;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Order.Request
{
    /// <summary>
    /// 提交给我的
    /// </summary>
    public class OrderSubmtToMeReq:PageReq
    {
        
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
       public string types { get; set; }
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

        public string base_entry { get; set; }

        public string job_id { get; set; }

        public string job_nm { get; set; }

        public string job_state { get; set; }

        public string remarks { get; set; }

        public string sbo_itf_return { get;set; }
    }
}
