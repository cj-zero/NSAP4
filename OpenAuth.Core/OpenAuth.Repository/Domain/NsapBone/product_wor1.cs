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
    [Table("product_wor1")]
    public partial class product_wor1 : Entity
    {
        public product_wor1()
        {
          this.ItemCode= string.Empty;
          this.IssueType= string.Empty;
          this.wareHouse= string.Empty;
          this.WipActCode= string.Empty;
          this.OcrCode= string.Empty;
          this.Project= string.Empty;
          this.U_ZWLKC= string.Empty;
          this.U_ZWLCN= string.Empty;
          this.U_WFKC= string.Empty;
          this.U_JGF_DJ= string.Empty;
          this.U_DrawingVer= string.Empty;
        }


        public int sbo_id { get; set; }
        public int DocEntry { get; set; }
        public int LineNum { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string ItemCode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? BaseQty { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? PlannedQty { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? IssuedQty { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string IssueType { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string wareHouse { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? VisOrder { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string WipActCode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? CompTotal { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string OcrCode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? LocCode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public short? LoadFrBOM { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string Project { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string U_ZWLKC { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string U_ZWLCN { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string U_WFKC { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string U_JGF_DJ { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string U_DrawingVer { get; set; }
    }
}