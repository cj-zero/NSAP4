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

namespace OpenAuth.Repository.Domain.Sap
{
    /// <summary>
	/// 
	/// </summary>
    [Table("OWDD")]
    public partial class OWDD : Entity
    {
        public OWDD()
        {
          this.ObjType= string.Empty;
          this.DocDate= DateTime.Now;
          this.Status= string.Empty;
          this.Remarks= string.Empty;
          this.CreateDate= DateTime.Now;
          this.IsDraft= string.Empty;
          this.DraftType= string.Empty;
          this.BFType= string.Empty;
          this.ProcesStat= string.Empty;
          this.StpUpdated= string.Empty;
        }

        
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? WtmCode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? OwnerID { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? DocEntry { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string ObjType { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public System.DateTime? DocDate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? CurrStep { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string Status { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string Remarks { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public short? UserSign { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public System.DateTime? CreateDate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public short? CreateTime { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string IsDraft { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public short? MaxReqr { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public short? MaxRejReqr { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? SrcDocEnt { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string DraftType { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? DraftEntry { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string BFType { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? ProcessID { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string ProcesStat { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string StpUpdated { get; set; }
    }
}