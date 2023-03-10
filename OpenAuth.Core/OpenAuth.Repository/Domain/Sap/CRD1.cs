//------------------------------------------------------------------------------
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
    [Table("CRD1")]
    public partial class CRD1
    {
        public CRD1()
        {
          this.Street= string.Empty;
          this.Block= string.Empty;
          this.ZipCode= string.Empty;
          this.City= string.Empty;
          this.County= string.Empty;
          this.Country= string.Empty;
          this.State= string.Empty;
          this.ObjType= string.Empty;
          this.LicTradNum= string.Empty;
          this.TaxCode= string.Empty;
          this.Building= string.Empty;
          this.Address2= string.Empty;
          this.Address3= string.Empty;
          this.AddrType= string.Empty;
          this.StreetNo= string.Empty;
          this.AltCrdName= string.Empty;
          this.AltTaxId= string.Empty;
          this.TaxOffice= string.Empty;
          this.GlblLocNum= string.Empty;
          this.Ntnlty= string.Empty;
          this.DIOTNat= string.Empty;
          this.TaaSEnbl= string.Empty;
          this.GSTRegnNo= string.Empty;
          this.CreateDate= DateTime.Now;
          this.EncryptIV= string.Empty;
          this.MYFType= string.Empty;
          this.U_PRX_SID= string.Empty;
          this.U_Active= string.Empty;
        }

        
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string Street { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string Block { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string ZipCode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string City { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string County { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string Country { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string State { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public short? UserSign { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? LogInstanc { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string ObjType { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string LicTradNum { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? LineNum { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string TaxCode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string Building { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string Address2 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string Address3 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string AddrType { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string AdresType { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string StreetNo { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string AltCrdName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        [Browsable(false)]
        public string AltTaxId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string TaxOffice { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string GlblLocNum { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string Ntnlty { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string DIOTNat { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string TaaSEnbl { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string GSTRegnNo { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? GSTType { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public System.DateTime? CreateDate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? CreateTS { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string EncryptIV { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string MYFType { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string U_PRX_SID { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string U_Active { get; set; }

        [Description("客户编号")]
        public string CardCode { get; set; }
        [Description("地址标识")]
        public string Address { get; set; }
    }
}