using OpenAuth.Repository.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace OpenAuth.Repository.Domain
{
    [Table("internalcontactserviceorder")]
    public class InternalContactServiceOrder : Entity
    {
        public InternalContactServiceOrder()
        {
            this.ItemCode = string.Empty;
            this.MnfSerial = string.Empty;
            this.CardCode = string.Empty;
            this.CardName = string.Empty;
            this.Supervisor = string.Empty;
            this.Status = string.Empty;
            this.FromTheme = string.Empty;
        }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        [Browsable(false)]
        public int? InternalContactId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string ItemCode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string ItemName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string MnfSerial { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string FromTheme { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        [Browsable(false)]
        public int? ServiceOrderId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        [Browsable(false)]
        public int? ServiceOrderSapId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string CardCode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string CardName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string Supervisor { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string Status { get; set; }
    }
}
