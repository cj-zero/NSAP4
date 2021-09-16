using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Material.Response
{
    public class CardAddressResp
    {
        public string CardCode { get; set; }

        public decimal? Balance { get; set; }

        public string frozenFor { get; set; }

        public string BillingAddress { get; set; }

        public string DeliveryAddress { get; set; }
    }
}
