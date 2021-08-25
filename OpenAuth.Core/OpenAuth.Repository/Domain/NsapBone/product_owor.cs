using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace OpenAuth.Repository.Domain
{
    /// <summary>
    /// 
    /// </summary>

    [Table("product_owor")]
    public partial class product_owor 
    {
        public product_owor()
        {
          this.DocNum= 0;
          this.ItemCode= string.Empty;
          this.Status= string.Empty;
          this.Type= string.Empty;
          this.PostDate= DateTime.Now;
          this.DueDate= DateTime.Now;
          this.OriginType= string.Empty;
          this.Comments= string.Empty;
          this.CloseDate= DateTime.Now;
          this.RlsDate= DateTime.Now;
          this.CardCode= string.Empty;
          this.Warehouse= string.Empty;
          this.Uom= string.Empty;
          this.JrnlMemo= string.Empty;
          this.CreateDate= DateTime.Now;
          this.Printed= string.Empty;
          this.OcrCode= string.Empty;
          this.PIndicator= string.Empty;
          //this.UpdateDate= DateTime.Now;
          this.Project= string.Empty;
          this.SupplCode= string.Empty;
          this.U_ZS= string.Empty;
          this.U_XT_CZ= string.Empty;
          this.U_WO_LTDW= string.Empty;
          this.U_BOM_BBH= string.Empty;
          this.U_job_id= string.Empty;
          this.U_SC_LB= string.Empty;
          this.U_BDFW= 0;
          this.CDocEntry= string.Empty;
          this.txtitemName= string.Empty;
          this.U_TZVer_UpdTime= DateTime.Now;
        }

        public int sbo_id { get; set; }
        public int DocEntry { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int DocNum { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public short? Series { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string ItemCode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string Status { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string Type { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? PlannedQty { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? CmpltQty { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? RjctQty { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public System.DateTime? PostDate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public System.DateTime? DueDate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? OriginAbs { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? OriginNum { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string OriginType { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public short? UserSign { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string Comments { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public System.DateTime? CloseDate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public System.DateTime? RlsDate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string CardCode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string Warehouse { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string Uom { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? LineDirty { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? WOR1Count { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string JrnlMemo { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        [Browsable(false)]
        public int? TransId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public System.DateTime? CreateDate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string Printed { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string OcrCode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string PIndicator { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public System.DateTime? UpdateDate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string Project { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string SupplCode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string U_ZS { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string U_XT_CZ { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string U_WO_LTDW { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string U_BOM_BBH { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string U_job_id { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? U_JGF { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? U_JYF { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal? U_TZF { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string U_SC_LB { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public decimal U_BDFW { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string CDocEntry { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public string txtitemName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public int? U_TZVer_complete { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Description("")]
        public System.DateTime? U_TZVer_UpdTime { get; set; }
    }
}