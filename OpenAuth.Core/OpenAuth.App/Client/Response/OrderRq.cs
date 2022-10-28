using NSAP.Entity.Client;
using OpenAuth.Repository.Domain;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace OpenAuth.App.Client.Response
{
    public class OrderRq
    {
        public decimal Total { set; get; }
        public decimal OpenDocTotal { set; get; }
        public DataTable dt { set; get; }

        public int count { set; get; }
    }

    /// <summary>
    /// 审批页面额外字段
    /// </summary>
    public class AuditCode
    {
        /// <summary>
        /// 机会编码
        /// </summary>
        public string base_entry { set; get; }
        /// <summary>
        /// 售后主管部门
        /// </summary>
        public string DfTcnician_dept { set; get; }
        /// <summary>
        /// 业务员部门
        /// </summary>
        public string SlpName_dept { set; get; }
        /// <summary>
        /// 申请人部门
        /// </summary>
        public string Applicant_dept { set; get; }
    }
}
