using System;
using System.Collections.Generic;
using System.Text;
using OpenAuth.App.Request;

namespace OpenAuth.App.Client.Request
{
    public class ClientListReq : PageReq
    {
        public string qtype { get; set; }
        public string query { get; set; }
        public string sortname { get; set; }
        public string sortorder { get; set; }
    }
    /// <summary>
    /// 查询所有技术员model
    /// </summary>
    public class GetTcnicianInfoReq : PageReq
    {
        public string qtype { get; set; }
        public string query { get; set; }
        public string sortname { get; set; }
        public string sortorder { get; set; }
        public string SboId { get; set; }


    }
    ///查询 国家·省·市
    public class GetStateProvincesInfoReq : PageReq
    {
        public string qtype { get; set; }
        public string query { get; set; }
        public string sortname { get; set; }
        public string sortorder { get; set; }
        public string AddrType { get; set; }
        public string CountryId { get; set; }
        public string StateId { get; set; }

    }
}
