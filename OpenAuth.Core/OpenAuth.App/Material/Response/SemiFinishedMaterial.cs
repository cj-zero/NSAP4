using Infrastructure.AutoMapper;
using OpenAuth.Repository.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Response
{
    [AutoMapTo(typeof(product_owor_wor1))]
    public class SemiFinishedMaterial: product_owor_wor1
    {
        public SemiFinishedMaterial()
        {
            this.FinilishedItems = new List<FinilishedItem>();
        }
        public List<FinilishedItem> FinilishedItems { get; set; }
    }

    /// <summary>
    /// 成品物料
    /// </summary>
    public class FinilishedItem
    {
        public int? DocEntry { get; set; }
        public string PartItemCode { get; set; }
        public decimal? PartPlannedQty { get; set; }
        public string productionOrg { get; set; }
        public string productionOrgManager { get; set; }
        public string WareHouse { get; set; }
        public decimal? PartQty { get; set; }
    }

    public class WarehouseBasics
    {
        public string WhsCode { get; set; }
        public decimal? OnHand { get; set; }
    }
}
