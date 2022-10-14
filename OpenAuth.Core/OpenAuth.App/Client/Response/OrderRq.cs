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
}
