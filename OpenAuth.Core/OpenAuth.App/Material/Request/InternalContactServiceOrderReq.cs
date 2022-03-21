using Infrastructure.AutoMapper;
using OpenAuth.Repository.Domain;
using OpenAuth.Repository.Domain.Material;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Material.Request
{
    [AutoMapTo(typeof(InternalContactServiceOrder))]
    public class InternalContactServiceOrderReq
    {
        public InternalContactServiceOrderReq()
        {
            this.Status = "未完成";
        }
        public string ItemCode { get; set; }
        public string ItemName { get; set; }
        public string MnfSerial { get; set; }
        public string FromTheme { get; set; }
        public string CardCode { get; set; }
        public string CardName { get; set; }
        public string Supervisor { get; set; }
        public string Status { get; set; }
    }
}
