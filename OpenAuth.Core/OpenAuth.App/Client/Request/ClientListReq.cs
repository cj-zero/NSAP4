﻿using System;
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
    /// <summary>
    /// 客户下销售报价单
    /// </summary>
    public class SelectOqutReq 
    {

        public string Docentry { get; set; }
        public string Slpname { get; set; }
        public string Status { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public string CardCode { get; set; }
    }
    /// <summary>
    /// 客户下销售订单
    /// </summary>
    public class SelectOrdrReq
    {

        public string Docentry { get; set; }
        public string Slpname { get; set; }
        public string Status { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public string CardCode { get; set; }
    }
}