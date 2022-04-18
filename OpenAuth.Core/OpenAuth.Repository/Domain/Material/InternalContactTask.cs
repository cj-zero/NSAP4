﻿//------------------------------------------------------------------------------
// <autogenerated>
//     This code was generated by a CodeSmith Template.
//
//     DO NOT MODIFY contents of this file. Changes to this
//     file will be lost if the code is regenerated.
//     Author:Yubao Li
// </autogenerated>
//------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using OpenAuth.Repository.Core;

namespace OpenAuth.Repository.Domain
{
    /// <summary>
	/// 
	/// </summary>
    [Table("internalcontacttask")]
    public partial class InternalContactTask : Entity
    {
        public InternalContactTask()
        {
          this.ItemCode= string.Empty;
          this.FromTheme= string.Empty;
          this.ProductionOrg= string.Empty;
          this.ProductionOrgManager= string.Empty;
          this.WareHouse= string.Empty;
            this.FinishedQty = 0;
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
        public string FromTheme { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        [Browsable(false)]
        public int? ProductionId { get; set; }
        //public string ProductionOrgId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string ProductionOrg { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string ProductionOrgManager { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string WareHouse { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? FinishedQty { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? BelongQty { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? RectifyQty { get; set; }
        public int? ServiceOrderId { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }
    }
}