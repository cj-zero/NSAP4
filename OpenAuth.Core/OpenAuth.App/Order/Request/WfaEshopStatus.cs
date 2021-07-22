using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Order.Request
{
    public class WfaEshopStatus
    {
        /// <summary>
        /// 合同Id
        /// </summary>
        public int DocumentId { get; set; }
        public int JobId { get; set; }
        public int? UserId { get; set; }
        public int? SlpCode { get; set; }
        public string QuotationEntry { get; set; }
        public string OrderEntry { get; set; }
        public string CardCode { get; set; }
        public string CardName { get; set; }
        public int? CurStatus { get; set; }
        public string OrderPhase { get; set; }
        public string ShippingPhase { get; set; }
        public string CompletePhase { get; set; }
        public DateTime? FirstCreateDate { get; set; }
        public DateTime? OrderLastDate { get; set; }
        public DateTime? ShippingLastDate { get; set; }
        public DateTime? CompleteLastDate { get; set; }
        /// <summary>
        /// 明细
        /// </summary>
        public List<AddWfaEshopOqutdetailDto> Items { get; set; }
    }
    public class AddWfaEshopStatusDto : WfaEshopStatus
    {
        /// <summary>
        /// 明细
        /// </summary>
        public List<AddWfaEshopOqutdetailDto> Items { get; set; }
    }
    public class UpdateWfaEshopStatusDto : WfaEshopStatus
    {
        /// <summary>
        /// 合同Id
        /// </summary>
        public int DocumentId { get; set; }
        /// <summary>
        /// 明细
        /// </summary>
        public List<AddWfaEshopOqutdetailDto> Items { get; set; }
    }
    public class AddWfaEshopOqutdetailDto
    {
        public int DocumentId { get; set; }
        public string ItemCode { get; set; }
        public string ItemDesc { get; set; }
        public decimal? ItemQty { get; set; }
    }
}
